using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Extensions;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Services.Helpers;
using nordelta.cobra.webapi.Utils;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.webapi.Services
{
    public class PaymentsFilesService : IPaymentsFilesService
    {
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly IPaymentService _paymentService;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IUserService _userService;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;
        private readonly IRepository<PublishedDebtFile> _publishDebtFileRepo;
        private readonly ServiciosMonitoreadosConfiguration _servicios;
        private readonly IUserChangesLogRepository _userChangesLogRepository;
        private readonly IUserRepository _userRepository;

        public PaymentsFilesService(
                                    IArchivoDeudaRepository archivoDeudaRepository,
                                    IPaymentService paymentService,
                                    IEmpresaRepository empresaRepository,
                                    IBackgroundJobClient backgroundJobClient,
                                    IUserService userService,
                                    IMailService mailService,
                                    IConfiguration configuration,
                                    IRepository<PublishedDebtFile> publishDebtFileRepo,
                                    IOptions<ServiciosMonitoreadosConfiguration> servicesMonConfig,
                                    IUserChangesLogRepository userChangesLogRepository,
                                    IUserRepository userRepository
        )
        {
            _archivoDeudaRepository = archivoDeudaRepository;
            _paymentService = paymentService;
            _empresaRepository = empresaRepository;
            _backgroundJobClient = backgroundJobClient;
            _userService = userService;
            _mailService = mailService;
            _configuration = configuration;
            _publishDebtFileRepo = publishDebtFileRepo;
            _servicios = servicesMonConfig.Value;
            _userChangesLogRepository = userChangesLogRepository;
            _userRepository = userRepository;
        }

        [Queue("files")]
        [JobDisplayName("PublicacionDeDeuda")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public void ProcessAllFiles()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PaymentsFiles");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            foreach (string file in Directory.GetFiles(filePath))
            {
                _backgroundJobClient.Enqueue(() => ProcessSingleFileAsync(file));
            }
        }

        [Queue("mssql")]
        [JobDisplayName("ProcessSingleFile")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task ProcessSingleFileAsync(string file)
        {
            var fileName = Path.GetFileName(file);
            var dateNow = LocalDateTime.GetDateTimeNow();
            try
            {
                ArchivoDeuda existingFile = await _archivoDeudaRepository.GetByFileNameAsync(Path.GetFileNameWithoutExtension(file));
                if (existingFile == null)
                {
                    List<DetalleDeuda> detallesForThisFile = await ProcessPaymentFileAsync(file);

                    var repeatedDebtDetails = detallesForThisFile.GroupBy(x => new
                    {
                        x.ArchivoDeudaId,
                        NroComprobante = x.NroComprobante.ToLowerInvariant(),
                        x.FechaPrimerVenc,
                        x.CodigoMoneda,
                        x.ObsLibreSegunda
                    })
                    .Where(x => x.Skip(1).Any())
                    .SelectMany(x => x).ToList();

                    var filteredDetails = detallesForThisFile.Except(repeatedDebtDetails, new DistinctDetalleDeudaComparer()).ToList();

                    await _archivoDeudaRepository.AddRepeatedDebtDetailsAsync(repeatedDebtDetails
                        .Select(x => new RepeatedDebtDetail()
                        {
                            CodigoMoneda = x.CodigoMoneda,
                            CodigoProducto = x.ObsLibreCuarta,
                            CodigoTransaccion = x.ObsLibreSegunda,
                            FechaPrimerVenc = x.FechaPrimerVenc,
                            FileName = x.ArchivoDeuda.FileName,
                            NroComprobante = x.NroComprobante,
                            NroCuitCliente = x.NroCuitCliente,
                            RazonSocialCliente = x.NombreCliente,
                            IdClienteOracle = x.ObsLibrePrimera.Split('|').First(),
                            IdSiteOracle = x.ObsLibrePrimera.Split('|').Last()
                        })
                    );

                    await _archivoDeudaRepository.AddDetalleAsync(filteredDetails);

                    var res = _publishDebtFileRepo.Insert(new PublishedDebtFile()
                    {
                        DebtFileName = fileName,
                        Success = true,
                        CreatedOn = dateNow
                    });

                    if (res > 0)
                        Monitoreo.Monitor.Ok($"ProcessSingleFileAsync(): Se proceso correctamente {fileName} publicación deuda COBRA", _servicios.PubDeudaCobra);
                    else
                        Monitoreo.Monitor.Critical($"ProcessSingleFileAsync(): Ocurrió un error al momento de intentar procesar {fileName}", _servicios.PubDeudaCobra);

                }
            }
            catch (Exception ex)
            {
                Monitoreo.Monitor.Critical($"ProcessSingleFileAsync(): Ocurrió un error al momento de intentar procesar {fileName}", _servicios.PubDeudaCobra);
                _publishDebtFileRepo.Insert(new PublishedDebtFile()
                {
                    DebtFileName = fileName,
                    Success = false,
                    CreatedOn = dateNow,
                    Error = ex.InnerException != null ? ex.InnerException.GetBaseException().Message : ex.Message
                });
                throw;
            }
        }

        public IEnumerable<PublishedDebtFile> GetAllPublishedDebtFiles()
        {
            return _publishDebtFileRepo.GetAll().OrderByDescending(x => x.CreatedOn);
        }

        public async Task<List<DetalleDeuda>> ProcessPaymentFileAsync(string filePath)
        {
            List<DetalleDeuda> listDetalleDeuda = new List<DetalleDeuda>();

            if (File.Exists(filePath))
            {
                string[] lines = (await File.ReadAllLinesAsync(filePath)).ToArray();
                MapDetallesDeuda(listDetalleDeuda, filePath, lines);
            }

            return listDetalleDeuda;
        }

        [Queue("rejectionfiles")]
        [JobDisplayName("PublicacionDeDeudaErrores")]
        [DisableConcurrentExecution(timeoutInSeconds: 1800)]
        public void ProcessAllRejectionFiles()
        {
            Serilog.Log.Information("Procesando archivos de rechazos del Santander...");

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PaymentsFilesRejections");

            CheckPaymentRejectionDirectory(filePath);

            if (Directory.GetFiles(filePath).Length > 0)
            {
                foreach (string file in Directory.GetFiles(filePath))
                {
                    //PurgeQueue("rejectionFiles");
                    PublishDebtRejectionFile publishDebtRejectionFile = new PublishDebtRejectionFile
                    {
                        FileName = Path.GetFileName(file),
                        FileDate = File.GetCreationTime(file)
                    };
                    _backgroundJobClient.Enqueue(() => ProcessSingleRejectionFileAsync(file, publishDebtRejectionFile));
                }
            }
            else
                Serilog.Log.Information("No hay archivos de rechazo para procesar...");
        }

        [Queue("mssql")]
        [JobDisplayName("ProcessSingleRejectionFile")]
        [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]

        public async Task ProcessSingleRejectionFileAsync(string filePath, PublishDebtRejectionFile publishDebtRejectionFile)
        {
            try
            {
                List<PublishDebtRejection> listPublishDebtRejection = new List<PublishDebtRejection>();

                if (File.Exists(filePath))
                {
                    string[] lines = (await File.ReadAllLinesAsync(filePath)).ToArray();
                    MapPublishDebtRejection(listPublishDebtRejection, filePath, lines);
                }

                publishDebtRejectionFile.PublishDebtRejections = listPublishDebtRejection;
                _archivoDeudaRepository.SavePublicDebtRejectionFile(publishDebtRejectionFile);

                //Una vez procesado borro el archivo
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Error procesando archivo de rechazo del Santander {file}: {@error}", publishDebtRejectionFile.FileName, ex);
            }
        }

        [Queue("mssql")]
        public async Task SendRepeatedDebtDetailsEmail()
        {
            if (_configuration.GetSection("ServiceConfiguration:SendRepetedDebtDetailsEmail").Get<bool>())
            {
                var details = (await _archivoDeudaRepository.GetAllRepeatedDebtDetailsAsync()).ToList();
                var filteredDetails = details.Distinct(new DistinctDebtDetailComparer()).OrderBy(x => x.FileName).ThenBy(x => x.NroComprobante).ThenBy(x => x.NroCuitCliente).ToList();
                var emails = _userService.GetEmailsForRole("CuentasACobrar").ToList();
                if (filteredDetails.Any() && emails.Any())
                {
                    await this._mailService.SendRepeatedDebtDetailsEmail(filteredDetails, emails);
                    _archivoDeudaRepository.CleanRepeatedDebtDetails();
                }
            }
        }

        private void MapPublishDebtRejection(List<PublishDebtRejection> listPublishDebtRejections, string filePath, string[] lines)
        {
            try
            {
                int index = 0;
                string Empresa = lines.First().Substring(1, 11);
                int UltimaRendicionProcesada = int.Parse(lines.First().Substring(26, 5));
                foreach (var line in lines)
                {
                    if (index == 0)
                    {
                        index++;
                        continue;
                    }

                    if (index != lines.Length - 1)
                    {
                        PublishDebtRejection publishDebtRejection = new PublishDebtRejection();
                        List<PublishDebtRejectionError> publishDebtRejectionErrors = new List<PublishDebtRejectionError>();

                        publishDebtRejection.Empresa = Empresa;
                        publishDebtRejection.UltimaRendicionProcesada = UltimaRendicionProcesada;
                        publishDebtRejection.Moneda = line.Substring(11, 1);
                        publishDebtRejection.NroCliente = line.Substring(12, 15);
                        publishDebtRejection.TipoComprobante = line.Substring(27, 2);
                        publishDebtRejection.NroComprobante = line.Substring(29, 15);
                        publishDebtRejection.NroCuota = line.Substring(44, 4).TryParse() ?? -1;
                        publishDebtRejection.CuitCliente = line.Substring(48, 11);

                        publishDebtRejectionErrors.AddRange(new List<PublishDebtRejectionError>() {
                            new PublishDebtRejectionError() {
                                CodigoError = line.Substring(185, 3).TryParse() ?? -1,
                                DescripcionError = line.Substring(188, 40).Trim()
                            }, new PublishDebtRejectionError() {
                                CodigoError = line.Substring(228, 3).TryParse() ?? -1,
                                DescripcionError = line.Substring(231, 40).Trim()
                            }, new PublishDebtRejectionError() {
                                CodigoError = line.Substring(271, 3).TryParse() ?? -1 ,
                                DescripcionError = line.Substring(274, 40).Trim()
                            }, new PublishDebtRejectionError() {
                                CodigoError = line.Substring(314, 3).TryParse() ?? -1,
                                DescripcionError = line.Substring(317, 40).Trim()
                            }, new PublishDebtRejectionError() {
                                CodigoError = line.Substring(357, 3).TryParse() ?? -1,
                                DescripcionError = line.Substring(360, 40).Trim()
                            }
                        });

                        //Borro los errores vacios
                        publishDebtRejectionErrors = publishDebtRejectionErrors.Where(item => item.CodigoError != -1).ToList();
                        publishDebtRejection.Errors = publishDebtRejectionErrors;

                        listPublishDebtRejections.Add(publishDebtRejection);
                    }

                    index++;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Error procesando archivo de rechazo del Santander: {@error}", ex);
            }
        }

        private void MapDetallesDeuda(List<DetalleDeuda> listDetalleDeuda, string filePath, string[] lines)
        {
            string firstLine = lines.First();
            HeaderDeuda header = new HeaderDeuda();
            header.TipoRegistro = firstLine.Substring(0, 1);
            header.CodOrganismo = firstLine.Substring(1, 17);
            header.CodCanal = firstLine.Substring(18, 3);
            header.NroEnvio = firstLine.Substring(21, 5);
            header.UltimaRendicionProcesada = firstLine.Substring(26, 5);
            header.MarcaActualizacionCuentaComercial = firstLine.Substring(31, 1);
            header.MarcaPublicacionOnline = firstLine.Substring(32, 1);
            header.Relleno = firstLine.Substring(33, 617);

            OrganismoDeuda organismo = new OrganismoDeuda();
            organismo.CuitEmpresa = firstLine.Substring(1, 11);
            organismo.NroDigitoEmpresa = firstLine.Substring(12, 1);
            organismo.CodProducto = firstLine.Substring(13, 3);
            organismo.NroAcuerdo = firstLine.Substring(16, 2);
            header.Organismo = organismo;

            string lastLine = lines.Last();
            TrailerDeuda trailer = new TrailerDeuda();
            trailer.TipoRegistro = lastLine.Substring(0, 1);
            trailer.ImporteTotalPrimerVencimiento = lastLine.Substring(1, 15);
            trailer.Ceros = lastLine.Substring(16, 15);
            trailer.CantRegistroInformados = lastLine.Substring(31, 7);
            trailer.Relleno = lastLine.Substring(38, 612);

            ArchivoDeuda archivoDeuda = new ArchivoDeuda();
            archivoDeuda.FileName = Path.GetFileNameWithoutExtension(filePath);
            archivoDeuda.FormatedFileName = FormatFileName(archivoDeuda.FileName);
            archivoDeuda.TimeStamp = LocalDateTime.GetDateTimeNow().ToString(CultureInfo.InvariantCulture);
            archivoDeuda.Header = header;
            archivoDeuda.Trailer = trailer;

            for (int i = 1; i <= lines.Length - 2; i++)
            {
                string currentLine = lines[i];
                DetalleDeuda detalleDeuda = new DetalleDeuda();
                detalleDeuda.TipoRegistro = currentLine.Substring(0, 1);
                detalleDeuda.TipoOperacion = currentLine.Substring(1, 1);
                detalleDeuda.CodigoMoneda = currentLine.Substring(2, 1);
                detalleDeuda.NumeroCliente = currentLine.Substring(3, 15);
                detalleDeuda.TipoComprobante = currentLine.Substring(18, 2);
                detalleDeuda.NroComprobante = currentLine.Substring(20, 15);
                detalleDeuda.NroCuota = currentLine.Substring(35, 4);
                detalleDeuda.NombreCliente = currentLine.Substring(39, 30);
                detalleDeuda.DireccionCliente = currentLine.Substring(69, 30);
                detalleDeuda.DescripcionLocalidad = currentLine.Substring(99, 20);
                detalleDeuda.PrefijoCodPostal = currentLine.Substring(119, 1);
                detalleDeuda.NroCodPostal = currentLine.Substring(120, 5);
                detalleDeuda.UbicManzanaCodPostal = currentLine.Substring(125, 4);
                detalleDeuda.FechaPrimerVenc = currentLine.Substring(129, 8);
                detalleDeuda.ImportePrimerVenc = currentLine.Substring(137, 15);
                detalleDeuda.FechaSegundoVenc = currentLine.Substring(152, 8);
                detalleDeuda.ImporteSegundoVenc = currentLine.Substring(160, 15);
                detalleDeuda.FechaHastaDescuento = currentLine.Substring(175, 8);
                detalleDeuda.ImporteProntoPago = currentLine.Substring(183, 15);
                detalleDeuda.FechaHastaPunitorios = currentLine.Substring(198, 8);
                detalleDeuda.TasaPunitorios = currentLine.Substring(206, 6);
                detalleDeuda.MarcaExcepcionCobroComisionDepositante = currentLine.Substring(212, 1);
                detalleDeuda.FormasCobroPermitidas = currentLine.Substring(213, 10);
                detalleDeuda.NroCuitCliente = currentLine.Substring(223, 11);
                detalleDeuda.CodIngresosBrutos = currentLine.Substring(234, 1);
                detalleDeuda.CodCondicionIva = currentLine.Substring(235, 1);
                detalleDeuda.CodConcepto = currentLine.Substring(236, 3);
                detalleDeuda.DescCodigo = currentLine.Substring(239, 40);
                detalleDeuda.ObsLibrePrimera = currentLine.Substring(279, 18);
                detalleDeuda.ObsLibreSegunda = currentLine.Substring(297, 15);
                detalleDeuda.ObsLibreTercera = currentLine.Substring(312, 15);
                var ObsLibreCuarta = currentLine.Substring(327, 80).Split('|').ToList();
                detalleDeuda.ObsLibreCuarta = ObsLibreCuarta.First();
                if (ObsLibreCuarta.Count > 1)
                {
                    detalleDeuda.CodigoMonedaTc = ObsLibreCuarta.Last().Trim();
                }
                detalleDeuda.Relleno = currentLine.Substring(407, 242);
                detalleDeuda.ArchivoDeuda = archivoDeuda;

                listDetalleDeuda.Add(detalleDeuda);
            }

        }

        public List<PublishDebtRejectionFile> GetPublishDebtRejections(FilterReportByDatesViewModelRequest dates)
        {
            return _archivoDeudaRepository.GetPublishDebtRejectionFiles(dates.FechaDesde, dates.FechaHasta);
        }

        private static string FormatFileName(string fileName)
        {
            string[] fileNameArray = fileName.Split("_");
            fileNameArray = fileNameArray.Take(fileNameArray.Count() - 1).ToArray();
            string formatedFilename = string.Join("_", fileNameArray);

            return formatedFilename;
        }

        private static void PurgeQueue(string queueName)
        {
            var toDelete = new List<string>();
            var monitor = JobStorage.Current.GetMonitoringApi();

            var queue = monitor.Queues().FirstOrDefault(x => x.Name == queueName);
            if (queue != null)
            {
                for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
                {
                    monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
                        .ForEach(x => toDelete.Add(x.Key));
                }
                foreach (var jobId in toDelete)
                {
                    BackgroundJob.Delete(jobId);
                }
            }
        }

        public async Task<List<RepeatedDebsDetailsViewModel>> GetAllRepeatedDebtDetails()
        {
            var repeatedDebs = await _archivoDeudaRepository.GetAllRepeatedDebtDetailsAsync();

            return repeatedDebs
                .DistinctBy(it => it.IdClienteOracle)
                .Select(it => new RepeatedDebsDetailsViewModel
                {
                    Id = it.Id,
                    CodigoMoneda = it.CodigoMoneda,
                    CodigoProducto = it.CodigoProducto,
                    CodigoTransaccion = it.CodigoTransaccion,
                    NroComprobante = it.NroComprobante,
                    FechaPrimerVenc = DateTime.ParseExact(it.FechaPrimerVenc, "yyyyMMdd", null),
                    IdClienteOracle = it.IdClienteOracle,
                    IdSiteOracle = it.IdSiteOracle,
                    NroCuitCliente = it.NroCuitCliente,
                    RazonSocialCliente = it.RazonSocialCliente
                }).ToList();
        }

        public async Task<IEnumerable<AdvanceFeeDto>> GetAdvanceFeeOrdersAsync()
        {
            List<AdvanceFeeDto> advanceFeeDtos = new List<AdvanceFeeDto>();
            var advanceFees = await _archivoDeudaRepository.GetAdvancedFeesAsync();

            var users = _userRepository.GetUsersByIds(advanceFees.Select(x => x.UserId).Distinct().ToList());
            var cuits = _userRepository.GetUsersByCuits(advanceFees.Select(x => x.ClientCuit.ToString()).Distinct().ToList());

            foreach (var advanceFee in advanceFees)
            {
                var solicitante = users.FirstOrDefault(x => x.IdApplicationUser == advanceFee.UserId);
                var cliente = cuits.FirstOrDefault(x => x.Cuit == advanceFee.ClientCuit.ToString());

                var advanceFeeDto = new AdvanceFeeDto()
                {
                    Id = advanceFee.Id,
                    CodProducto = advanceFee.CodProducto,
                    UserId = advanceFee.UserId,
                    ClientCuit = advanceFee.ClientCuit.ToString(),
                    RazonSocial = cliente is null ? String.Empty : cliente.RazonSocial,
                    Solicitante = solicitante is null ? String.Empty : solicitante.RazonSocial,
                    Vencimiento = advanceFee.Vencimiento,
                    Moneda = advanceFee.Moneda,
                    Importe = advanceFee.Importe,
                    Saldo = advanceFee.Saldo,
                    Status = advanceFee.Status,
                    CreatedOn = advanceFee.CreatedOn
                };

                advanceFeeDtos.Add(advanceFeeDto);
            }

            return advanceFeeDtos;
        }

        public async Task<int?> PostOrderAdvanceFee(List<OrderAdvanceFeeViewModel> advanceFees, User user)
        {
            var advanceFeesRequest = new List<AdvanceFee>();
            var dateNow = LocalDateTime.GetDateTimeNow();

            foreach (var advanceFeeModel in advanceFees)
            {
                advanceFeesRequest.Add(new AdvanceFee
                {
                    UserId = user.Id,
                    CodProducto = advanceFeeModel.CodProducto,
                    ClientCuit = long.Parse(advanceFeeModel.Cuit),
                    Importe = advanceFeeModel.Importe,
                    Moneda = advanceFeeModel.Moneda,
                    Saldo = advanceFeeModel.Saldo,
                    Vencimiento = advanceFeeModel.Vencimiento,
                    Status = EAdvanceFeeStatus.Pendiente,
                    CreatedOn = dateNow,
                    Informed = false
                });

            }

            return await _archivoDeudaRepository.PostOrderAdvanceFees(advanceFeesRequest, user);
        }

        public async Task ChangeAdvanceFeeOrdersStatusAsync(List<int> ids, EAdvanceFeeStatus status)
        {
            await _archivoDeudaRepository.SetAdvanceFeeOrdersStatus(ids, status);
        }

        public void NotifyOneApprovedAdvanceFee(int orderId)
        {
            var sent = new List<int>();
            try
            {
                var advanceFee = _archivoDeudaRepository.GetAdvancedFeesByOrderId(orderId, x => !x.Informed.Value);
                if (advanceFee != null)
                {
                    var businessUnitByProductCode = _paymentService.
                        GetBusinessUnitByProductCodes(new List<string> { advanceFee.CodProducto.Trim() });

                    var result = new
                    {
                        AdvanceFeeId = advanceFee.Id,
                        CuitCliente = advanceFee.ClientCuit,
                        Importe = advanceFee.Importe,
                        Moneda = advanceFee.Moneda,
                        Vencimiento = advanceFee.Vencimiento,
                        Producto = advanceFee.CodProducto.Trim(),
                        BU = businessUnitByProductCode.Select(x => x.BusinessUnit)
                    };

                    var recipients = new List<string>();
                    var roles = new List<string>();


                    //Get additional billing department email addresses to notify
                    recipients = _configuration.GetSection("ServiceConfiguration:BillingEmailAddresses")
                        .Get<List<string>>();



                    var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
                    nfi.NumberGroupSeparator = ".";
                    nfi.NumberDecimalSeparator = ",";


                    var sb = new StringBuilder();
                    var user = _userRepository.GetUserById(advanceFee.UserId);
                    recipients.Add(user.Email);

                    sb.Append(
                        $"<p>El cliente con cuit {result.CuitCliente} solicita adelanto de cuota del producto {result.Producto} " +
                        $"con vencimiento {result.Vencimiento:dd/MM/yyyy} y un saldo de {result.Moneda} {(Convert.ToDecimal(advanceFee.Importe)).ToString("n", nfi)}</p>");
                    sent.Add(result.AdvanceFeeId);

                    var bu = _paymentService.GetBusinessUnitByProductCodes(new List<string> { advanceFee.CodProducto });

                    var empresa = _empresaRepository.GetByName(bu?.FirstOrDefault().BusinessUnit);
                    sb.Append($"<img src=\"{empresa?.Firma}\">");

                    _mailService.SendNotificationEmail(recipients, $"Adelantos de Cuotas {EAdvanceFeeStatus.Aprobado.ToString()}s", sb.ToString());

                }
                else
                {
                    Serilog.Log.Information("No AdvanceFeeOrders where found for notification");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Error Running Job NotifyAdvanceFeeOrders: {@err}", ex);
            }
            finally
            {
                _archivoDeudaRepository.UpdateInformedAdvanceFeeByIds(sent);
            }
        }

        private void CheckPaymentRejectionDirectory(string filePath)
        {
            try
            {
                Serilog.Log.Information("Checking if PaymentsFilesRejections folder exists...");
                if (!Directory.Exists(filePath))
                {
                    Serilog.Log.Information("PaymentsFilesRejections folder does not exists, creating...");
                    Directory.CreateDirectory(filePath);
                }
                else
                    Serilog.Log.Information("PaymentsFilesRejections folder already exists");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("Error creating or checking PaymentsFilesRejections folder: {@error}", ex);
            }
        }
    }
}
