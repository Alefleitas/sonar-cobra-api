using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DebitoInmediatoServiceItau;
using Microsoft.Extensions.Configuration;
using Serilog;
using nordelta.cobra.webapi.Utils;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using nordelta.cobra.webapi.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Configuration;
using RestSharp;

namespace nordelta.cobra.webapi.Services.Mocks
{
    public class ItauServiceMock : IItauService
    {
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly List<CertificateItem> _certificateItems;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ItauWCFConfiguration> _itauConfiguration;

        public ItauServiceMock(
            IConfiguration configuration, 
            IMailService mailService,
            IRestClient restClient, 
            IOptionsMonitor<ApiServicesConfig> apiServicesConfig,
            IOptionsMonitor<ItauWCFConfiguration> options)
        {
            _configuration = configuration;
            _mailService = mailService;
            _certificateItems = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CertificateItems").Get<List<CertificateItem>>();
            foreach (var certificate in _certificateItems)
                certificate.Password = AesManager.GetPassword(certificate.Password, configuration.GetSection("SecretKeyCertificate").Value);
            _restClient = restClient;
            _apiServicesConfig = apiServicesConfig;
            _itauConfiguration = options;
        }

        public async Task<RegistrarPublicacionResponse> RegisterPublicationAsync(RegisterPublicationDTO debinData, bool isAnonymousPayment = false)
        {
            RegistrarPublicacionRequest request = new RegistrarPublicacionRequest(
                new headerType()
                {
                    clave_mensaje = new clave_mensajeType()
                    {
                        id_requerimiento = "7012",
                        host_origen = "0.0.0.0",
                        id_canal = "Y"
                    },
                    info_requerimiento = new info_requerimientoType()
                    {
                        id_organizacion = "Itau",
                        codigo_sucursal = "0000",
                        fechahora_mensaje = debinData.Now.ToString("yyyy-MM-dd-HH:mm:ss.fffff") //DateTime Now
                    },
                    seguridad = new seguridadType()
                    {
                        token = new tokenType()
                        {
                            passwordtoken = new passwordtokenType()
                            {
                                id_usuario = "x",
                                clave = "x"
                            }
                        }
                    }
                },
                new bodyRequest()
                {
                    RegistrarPublicacion = new RegistrarPublicacionType()
                    {
                        datosVendedor = new datosVendedor()
                        {
                            numeroCuit = debinData.Vendedor.Cuit,
                            numeroCBU = (int)debinData.Moneda == 0 ? debinData.Vendedor.CbuPeso : debinData.Vendedor.CbuDolar,
                        },
                        datosComprador = new datosComprador()
                        {
                            numeroCuit = debinData.CompradorCuit,
                            numeroCBU = debinData.CompradorCbu,
                            codigo = "11111",
                            razonSocial = debinData.CompradorNombre
                        },
                        detalle = isAnonymousPayment ? new detalle()
                        {
                            concepto = "VAR",
                            comprobante = debinData.Vendedor.Cuit,
                            descripcion = debinData.Producto,
                            importe = Convert.ToDecimal(debinData.Importe),
                            moneda = (int)debinData.Moneda == 0 ? "ARP" : "USD",
                            recurrencia = "N",
                            fechaVencimiento = debinData.DueDate.ToString("yyyy-MM-dd"),
                            horaVencimiento = debinData.DueDate.ToString("HH:mm:ss"),
                        } : new detalle()
                        {
                            concepto = "VAR",
                            comprobante = debinData.Vendedor.Cuit,
                            descripcion = debinData.Producto,
                            importe = Convert.ToDecimal(debinData.Importe),
                            moneda = (int)debinData.Moneda == 0 ? "ARP" : "USD",
                            recurrencia = "N",
                            fechaVencimiento = debinData.DueDate.ToString("yyyy-MM-dd"),
                            horaVencimiento = debinData.DueDate.ToString("HH:mm:ss"),
                            claveExterna = debinData.ExternalCodeString
                        }
                    }
                }
            );

            Random random = new Random(Convert.ToInt32(debinData.Importe));
            Guid num = Guid.NewGuid();

            var result = new RegistrarPublicacionResponse(
                new headerType()
                {
                    transaccion = new transaccionType()
                    {
                        estado = "Exito"
                    }
                },
                new bodyResponseType()
                {
                    Item = new RegistrarPublicacionResponseType() { codigo = num.ToString("N").ToUpper() }
                }
            );

            Log.Debug(@"MockDebinSuccess, 
                    Type: MockPublishDebinSuccess,
                    Description: Mock Debin Published successful:
                    request: {@request},
                    response: {response}", request, result.header.transaccion.estado);

            return await Task.FromResult(result);
        }

        public async Task<PaymentStatus> GetDebinState(GetDebinStateRequest debinStateRequest)
        {
            try
            {
                ConsultarEstadoRequest request = new ConsultarEstadoRequest(
                    new headerType()
                    {
                        clave_mensaje = new clave_mensajeType()
                        {
                            id_requerimiento = "7012",
                            host_origen = "0.0.0.0",
                            id_canal = "W",
                        },
                        info_requerimiento = new info_requerimientoType()
                        {
                            id_organizacion = "Itau",
                            codigo_sucursal = "0000",
                            fechahora_mensaje = LocalDateTime.GetDateTimeNow().ToString("yyyy-MM-dd-HH:mm:ss.fffff"),
                        },
                        seguridad = new seguridadType()
                        {
                            token = new tokenType()
                            {
                                passwordtoken = new passwordtokenType()
                                {
                                    clave = "x",
                                    id_usuario = "x"
                                }
                            }
                        },
                    }, new bodyRequest1()
                    {
                        ConsultarEstado = new ConsultarEstadoType()
                        {
                            codigo = debinStateRequest.CodigoDebin,
                            numeroCBU = debinStateRequest.Cbu,
                            numeroCuit = debinStateRequest.Cuit
                        }
                    }
                );

                var customStateResponse = this._configuration.GetSection("ServiceConfiguration").GetSection("DebinStatusResponseMock").Get<string>();

                ConsultarEstadoResponse result = new ConsultarEstadoResponse(
                    new headerType()
                    {
                        transaccion = new transaccionType()
                        {
                            estado = "Exito"
                        }
                    },
                    new bodyResponseType1()
                    {
                        Item = new ConsultarEstadoResponseType() { descripcion = customStateResponse }
                    }
                );

                string state = "ENVIADO A COELSA";
                if (result.header.transaccion.estado.Equals("Exito"))
                {
                    state = ((ConsultarEstadoResponseType)result.body.Item).descripcion;
                    Log.Debug(@"MockGetDebinState, 
                            Type: MockGetDebinStateSuccess,
                            Description: Mock Get Debin State successful:
                            request: {@request},
                            response: {response}", request, result.header.transaccion.estado);
                }
                else
                {
                    Log.Debug(@"MockGetDebinState, 
                            Type: MockGetDebinStateFailed,
                            Description: Mock Failed Get Debin State:
                            request: {@request},
                            response: {response}", request, result.header.transaccion.estado);
                }

                PaymentStatus currentStatus;
                switch (state)
                {
                    case "ENVIADO A COELSA":
                        currentStatus = PaymentStatus.Pending;
                        break;
                    case "VENCIDO":
                        currentStatus = PaymentStatus.Expired;
                        break;
                    case "PAGADO":
                        currentStatus = PaymentStatus.Approved;
                        break;
                    case "RECHAZADO":
                        currentStatus = PaymentStatus.Rejected;
                        break;
                    case "ERROR":
                        currentStatus = PaymentStatus.Error;
                        break;
                    default:
                        currentStatus = PaymentStatus.Pending;
                        break;
                }

                return await Task.FromResult(currentStatus);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        

        public void CheckCertificateExpirationDate()
        {
            try
            {
                var checkSettingMonths = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CheckMonthsBeforeExpiration:CheckSetting").Get<CheckSetting>();
                var checkSettingWeeks = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CheckWeeksBeforeExpiration:CheckSetting").Get<CheckSetting>();
                var checkSettingDays = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CheckDaysBeforeExpiration:CheckSetting").Get<CheckSetting>();
                var certificatesToValidate = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CertificateItems").Get<List<CertificateItem>>();
                foreach (var certificate in certificatesToValidate)
                    certificate.Password = AesManager.GetPassword(certificate.Password, _configuration.GetSection("SecretKeyCertificate").Value);

                DateTime dueDateMonth = default;
                DateTime dueDateDays = default;
                DateTime dueDateWeeks = default;

                if (checkSettingDays.Enabled)
                    dueDateDays = LocalDateTime.GetDateTimeNow().Date.AddDays(checkSettingDays.Amount);
                if (checkSettingWeeks.Enabled)
                    dueDateWeeks = LocalDateTime.GetDateTimeNow().Date.AddDays((checkSettingWeeks.Amount * 7));
                if (checkSettingMonths.Enabled)
                    dueDateMonth = LocalDateTime.GetDateTimeNow().Date.AddMonths(checkSettingMonths.Amount);

                foreach (var certificateItem in certificatesToValidate)
                {
                    var certificate = GetCertificate(certificateItem);
                   
                    if (certificate == null)
                        continue;

                    var expirationDate = DateTime.Parse(certificate.GetExpirationDateString());
                    expirationDate = expirationDate.Date;

                    var body = string.Empty;

                    if (checkSettingDays.Enabled && expirationDate <= dueDateDays)
                    {
                        body = checkSettingDays.EmailSettings.Body.Replace("{certificateName}", certificateItem.Name);
                        var daysDiff = expirationDate.DayDiff(LocalDateTime.GetDateTimeNow());
                        body = body.Replace("{amount}", daysDiff + (daysDiff > 1 ? " dias" : " día" ));

                        _mailService.SendNotificationEmail(checkSettingDays.EmailSettings.EmailTo,
                            checkSettingDays.EmailSettings.Subject.Replace("{certificateName}", certificateItem.Name), body);
                    }
                    else if (checkSettingWeeks.Enabled && expirationDate <= dueDateWeeks)
                    {
                        body = checkSettingWeeks.EmailSettings.Body.Replace("{certificateName}", certificateItem.Name);
                        var weekDiff = LocalDateTime.GetDateTimeNow().WeekDiff(expirationDate);
                        body = body.Replace("{amount}", weekDiff + (weekDiff > 1 ? " semanas" : " semana"));

                        _mailService.SendNotificationEmail(checkSettingWeeks.EmailSettings.EmailTo,
                            checkSettingWeeks.EmailSettings.Subject.Replace("{certificateName}", certificateItem.Name),
                            body);

                    }
                    else if (checkSettingMonths.Enabled && expirationDate <= dueDateMonth)
                    {
                        body = checkSettingMonths.EmailSettings.Body.Replace("{certificateName}", certificateItem.Name);
                        var monthDiff = expirationDate.MonthDiff(LocalDateTime.GetDateTimeNow());
                        body = body.Replace("{amount}", monthDiff + (monthDiff > 1 ? " meses" : " mes"));

                        _mailService.SendNotificationEmail(checkSettingMonths.EmailSettings.EmailTo,
                            checkSettingMonths.EmailSettings.Subject.Replace("{certificateName}", certificateItem.Name),
                            body);
                    }
                    Serilog.Log.Information($"Certificate {certificateItem.Name} checked");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Error: CheckCertExpirationDate: {ex.Message}");
            }
        }

        private X509Certificate2 GetCertificate(CertificateItem certificateItem)
        {
            try
            {
                var certificationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates",
                    certificateItem.Name);
                var certificate = new X509Certificate2(certificationPath, certificateItem.Password,
                    X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet);
                return certificate;
            }
            catch (Exception e)
            {
                Serilog.Log.Error($"Error: al intentar abrir certificado: {e.Message}");
            }
            return null;
        }

        public OperationInformationResultDto CallItauGetOperationInformation(string getUri, string cuitPsp)
        {
            var certificateItem = _certificateItems
                .SingleOrDefault(it => it.VendorCuit == cuitPsp);

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.ItauApi).Url);
            _restClient.ClientCertificates = new X509CertificateCollection() { GetCertificate(certificateItem) };


            var request = new RestRequest(getUri, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomSerializer()
            };

            request.AddHeader("cuitPsp", cuitPsp);

            var response = _restClient.Execute<OperationInformationResultDto>(request);
            if (response.IsSuccessful)
            {
                Log.Information("CallItauGetOperationInformation a {@url} iniciado \n request: {@request}",
                    request.Resource, request);
                Log.Debug("CallItauGetOperationInformation to Itau \n request: {@request} \n response:{@response}",
                    request, response);
                return response.Data;
            }

            Log.Error("CallItauGetOperationInformation a {@url} . \n Response: {@response}", request.Resource,
                response);
            return null;
        }

        public TransactionResultDto CallItauApiCreateTransaction(RegisterCvuDto registerCvuDto, string cuitPsp)
        {
            var certificateItem = _certificateItems.SingleOrDefault(
                it =>
                    it.VendorCuit == cuitPsp);

            if (certificateItem == null)
            {
                throw new Exception($"Error: Al intentar obtener el certificado el Cuit de: {registerCvuDto.Cuit}");
            }

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.ItauApi).Url);
            _restClient.ClientCertificates = new X509CertificateCollection() { GetCertificate(certificateItem) };

            var request = new RestRequest("/transaction/cvu", Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomSerializer()
            };

            request.AddHeader("cuitPsp", cuitPsp);
            request.AddJsonBody(registerCvuDto);
            var response = _restClient.Execute<TransactionResultDto>(request);
            if (!response.IsSuccessful)
                throw new Exception(
                    $"Error: CallItauApiCreateTransaction, request: {JsonConvert.SerializeObject(registerCvuDto)} \n" +
                    $"response: {JsonConvert.SerializeObject(response.Content)}");

            Log.Information("CallItauApiCreateTransaction a {@url} iniciado \n request: {@request}", request.Resource,
                request);
            Log.Debug("CallItauApiCreateTransaction to Itau \n request: {@request} \n response:{@response}", request,
                response);
            return response.Data;
        }

        public TransactionResultDto CallItauApiGetCvuInformation(string getUri, string cuitPsp)
        {
            var certificateItem = _certificateItems.SingleOrDefault(
                it =>
                    it.VendorCuit == cuitPsp);

            _restClient.BaseUrl = new Uri(_apiServicesConfig.Get(ApiServicesConfig.ItauApi).Url);
            _restClient.ClientCertificates = new X509CertificateCollection() { GetCertificate(certificateItem) };


            var request = new RestRequest(getUri, Method.GET)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomSerializer()
            };
            request.AddHeader("cuitPsp", cuitPsp);

            var response = _restClient.Execute<TransactionResultDto>(request);
            if (response.IsSuccessful)
            {
                Log.Information("CallItauApiCreateTransaction a {@url} iniciado \n request: {@request}",
                    request.Resource, request);
                Log.Debug("CallItauApiCreateTransaction to Itau \n request: {@request} \n response:{@response}",
                    request, response);
                return response.Data;
            }

            Log.Error("CallItauApiCreateTransaction a {@url} . \n Response: {@response}", request.Resource, response);
            return null;
        }

    }
}
