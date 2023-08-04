using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;
using ArchivosCmlServiceItau;
using FileHelpers.MasterDetail;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Connected_Services.Itau.ArchivosCmlServiceItau.Constants;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.ItauFiles;
using nordelta.cobra.webapi.Models.ValueObject.BankFiles.Constants;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nordelta.cobra.webapi.Models.ValueObject.ItauPsp;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Models;
using Newtonsoft.Json;

namespace nordelta.cobra.webapi.Services
{
    public class ItauArchivosService : IItauArchivosService
    {
        private readonly IProcessTransactionService _processTransactionService;
        private readonly ICvuEntityRepository _cvuEntityRepository;
        private readonly List<ItauPspItem> _itauPspItems;
        private readonly IOptionsMonitor<ItauWCFConfiguration> _itauConfiguration;
        private readonly List<CertificateItem> _certificateItems;
        private ChannelFactory<ARCHIVOSCML> _channelFactoryArchivoCml;

        public ItauArchivosService(
            IConfiguration configuration,
            IOptionsMonitor<ItauWCFConfiguration> options,
            IProcessTransactionService processTransactionService,
            ICvuEntityRepository cvuEntityRepository)
        {
            _certificateItems = configuration.GetSection("ServiceConfiguration:CertificateSettings:CertificateItems").Get<List<CertificateItem>>();
            foreach (var certificate in _certificateItems)
                certificate.Password = AesManager.GetPassword(certificate.Password, configuration.GetSection("SecretKeyCertificate").Value);
            _itauPspItems = configuration.GetSection("ServiceConfiguration:CertificateSettings:ItauPSPIds").Get<List<ItauPspItem>>();
            _itauConfiguration = options;
            _processTransactionService = processTransactionService;
            _cvuEntityRepository = cvuEntityRepository;
        }

        public Task GetAndProcessPaymentFilesOfItau()
        {
            return Task.Run(async () =>
            {
                var debitoInmediatoConfig = _itauConfiguration.Get(ItauWCFConfiguration.ArchivosCmlConfiguration);
                var yesterday = LocalDateTime.GetDateTimeNow().AddDays(-1);

                var cvuCreationDates = _cvuEntityRepository.GetAll(x => x, y => y.Status == CvuEntityStatus.RegistrationStarted,
                                                                                           z => z.OrderBy(a => a.CreationDate)).Where(x => x.CreationDate.ToString("yyyy-MM-dd") != yesterday.ToString("yyyy-MM-dd"))
                                                                                           .GroupBy(x => new DateTime(x.CreationDate.Year, x.CreationDate.Month, x.CreationDate.Day),
                                                                                           (x, y) => y.FirstOrDefault().CreationDate).ToList();

                //// fechas de creacion de cvuEntities con status Started en la BD
                foreach (var creationDate in cvuCreationDates)
                {
                    foreach (var itauPspItem in _itauPspItems)
                    {
                        try
                        {
                            var response = await GetItauOperationsCashInArchivoRendiciones(itauPspItem, debitoInmediatoConfig, creationDate);
                            await ProcessItauOperationsCashInArchivoRendiciones(response, itauPspItem, creationDate);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Error al momento de obtener o procesar archivos de itau con itauPspItem: {JsonConvert.SerializeObject(itauPspItem)} y fecha: {creationDate:yyyy-MM-dd}");
                            continue;
                        }
                    }
                }

                // fecha de ayer
                foreach (var itauPspItem in _itauPspItems)
                {
                    try
                    {
                        var response = await GetItauOperationsCashInArchivoRendiciones(itauPspItem, debitoInmediatoConfig, yesterday);
                        await ProcessItauOperationsCashInArchivoRendiciones(response, itauPspItem, yesterday);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error al momento de obtener o procesar archivos de itau con itauPspItem: {JsonConvert.SerializeObject(itauPspItem)} y fecha: {yesterday:yyyy-MM-dd}");
                        continue;
                    }
                }
            });
        }

        public Task<archivosSolicitudResponse1> GetItauOperationsCashInArchivoRendiciones(ItauPspItem itauPspItem, ItauWCFConfiguration debitoInmediatoConfig, DateTime fechaGeneracion)
        {
            Log.Information("GetItauOperationsCashInArchivoRendiciones(): Solicitando archivos: fecha de generacion {fecha} - itauPspItem: {@itauPspItem}", fechaGeneracion, itauPspItem);

            var certConfig = GetItauCertificateConfig(itauPspItem.VendorCuit);
            _channelFactoryArchivoCml = GetConfiguratedChannel<ARCHIVOSCML>(debitoInmediatoConfig, certConfig);
            var serviceClient = _channelFactoryArchivoCml.CreateChannel();

            var req = new archivosSolicitudRequest(
                new archivosSolicitud()
                {
                    entrada = new inputSolicitudReporteValidacionEnvios()
                    {
                        convenio = new convenio()
                        {
                            cuenta = new cuenta()
                            {
                                deComision = "",
                                deProducto = "",
                                principal = ""
                            },
                            descripcion = "",
                            estado = "",
                            moneda = "",
                            cuit = itauPspItem.VendorCuit,
                            producto = new producto()
                            {
                                numero = itauPspItem.ProductoNumero
                            },
                            numero = itauPspItem.ConvenioNumero,
                        },
                        fechaGeneracion = fechaGeneracion.ToString("yyyyMMdd"),
                        tipoRendicion = TipoRendicion.RendicionTipoArchivoLimitado,
                        tipoArchivo = TipoArchivo.RendicionDiaria
                    }
                });

            return serviceClient.archivosSolicitudAsync(req);
        }

        public Task ProcessItauOperationsCashInArchivoRendiciones(archivosSolicitudResponse1 response, ItauPspItem itauPspItem, DateTime fechaGeneracion)
        {
            return Task.Run(() =>
            {
                if (response.archivosSolicitudResponse.retorno.codigo == CodigoRetorno.OkResult)
                {
                    Log.Information($"Se obtuvieron archivos desde itau para itauPspItem {JsonConvert.SerializeObject(itauPspItem)} tomando la fecha: {fechaGeneracion:yyyy-MM-dd}");

                    //todo llamar al proceso de levantar en memoria el attach que esta en zip y procesarlo.
                    var attachments = ProcessZipFromAttachment(response.archivosSolicitudResponse.attach);

                    // CVU REGISTER
                    if (attachments.Keys.Contains(ItauFilesConstants.CVFile))
                    {
                        _processTransactionService.ProcessRegistroFiles((List<CvRegistroCvu>)attachments[ItauFilesConstants.CVFile]);
                    }

                    // CVU OPERATION
                    if (attachments.Keys.Contains(ItauFilesConstants.PSFile))
                    {
                        _processTransactionService.ProcessRegistroFiles((List<PsPaymentItau>)attachments[ItauFilesConstants.PSFile]);
                    }

                    // ECHEQ OPERATION
                    if (attachments.Keys.Contains(ItauFilesConstants.NOFile))
                    {
                        var payments = new List<PaymentItau>();
                        var instruments = (List<PiItau>)attachments[ItauFilesConstants.NOFile];
                        var idPagos = instruments.Select(x => x.IdOperacion.Trim()).Distinct();

                        foreach (var idPago in idPagos)
                        {
                            payments.Add(new PaymentItau
                            {
                                Instruments = instruments.Where(x => x.IdOperacion == idPago).ToList()
                            });
                        }
                        _processTransactionService.ProcessRegistroFiles(payments);
                    }
                }
                else
                {
                    Log.Error($"Error: Al intentar obtener archivos desde Itau para itauPspItem {JsonConvert.SerializeObject(itauPspItem)} " +
                        $"tomando la fecha: {fechaGeneracion:yyyy-MM-dd}. " +
                        $"\n Response: {JsonConvert.SerializeObject(response)}");
                }
            });
        }

        private RecordAction SelectorMasterOrDetail(string record, string masterToken, string fileType)
        {
            if (record.Length < 2 || record[39].Equals('T'))
                return RecordAction.Skip;

            if ((record[39].Equals('P') || record[39].Equals('D')) && fileType.Equals(ItauFilesConstants.CDFile))
                return RecordAction.Skip;

            return record.Contains(masterToken) ? RecordAction.Master : RecordAction.Detail;
        }

        private Dictionary<string, dynamic> ProcessZipFromAttachment(byte[] attach)
        {
            var ret = new Dictionary<string, dynamic>();

            try
            {
                var memAttachment = new MemoryStream(attach);
                var zipArchive = new ZipArchive(memAttachment, ZipArchiveMode.Read);
                var entry = zipArchive.Entries.Where(it =>
                    it.Name.StartsWith(ItauFilesConstants.PSFile) || it.Name.StartsWith(ItauFilesConstants.CVFile) || it.Name.StartsWith(ItauFilesConstants.NOFile));

                foreach (var archivo in entry)
                {
                    try
                    {
                        using TextReader tr = new StreamReader(archivo.Open());
                        {
                            if (archivo.Name.StartsWith(ItauFilesConstants.CVFile))
                            {
                                var list = new List<CvRegistroCvu>();
                                var engine = new MasterDetailEngine<HeaderItau, CvRegistroCvu>(r =>
                                    SelectorMasterOrDetail(r, ItauFilesConstants.HeaderCVToken, ItauFilesConstants.CVFile));

                                var res = engine.ReadStream(tr);
                                foreach (var group in res)
                                {
                                    list.AddRange(@group.Details);
                                }

                                ret.Add(ItauFilesConstants.CVFile, list);
                                continue;
                            }

                            if (archivo.Name.StartsWith(ItauFilesConstants.PSFile))
                            {
                                var psPayments = new List<PsPaymentItau>();
                                var engine = new MasterDetailEngine<HeaderItau, PsRegistroCashIn>(r =>
                                    SelectorMasterOrDetail(r, ItauFilesConstants.HeaderPSToken, ItauFilesConstants.PSFile));

                                var res = engine.ReadStream(tr);
                                foreach (var group in res)
                                {
                                    foreach (var Detail in @group.Details)
                                    {
                                        Detail.FechaOperacion = DateTime.ParseExact(archivo.Name[22..30], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                                        psPayments.Add(new PsPaymentItau { Registro = Detail, Header = @group.Master });
                                    }
                                }

                                ret.Add(ItauFilesConstants.PSFile, psPayments);
                                continue;
                            }

                            if (archivo.Name.StartsWith(ItauFilesConstants.NOFile))
                            {
                                var list = new List<PiItau>();
                                var engine = new MasterDetailEngine<HeaderItau, PiItau>(r =>
                                    SelectorMasterOrDetail(r, ItauFilesConstants.HeaderNOToken, ItauFilesConstants.NOFile));

                                var res = engine.ReadStream(tr);

                                foreach (var group in res)
                                {
                                    list.AddRange(@group.Details);
                                }

                                ret.Add(ItauFilesConstants.NOFile, list);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al momento de procesar archivo: {fileName}", archivo.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error al intentar descomprimir el attach de ItauWS: {@e}", e);
            }

            return ret;
        }

        private CertificateItem GetItauCertificateConfig(string vendorCuit)
        {
            return _certificateItems.SingleOrDefault(it => it.VendorCuit == vendorCuit);
        }

        private static ChannelFactory<T> GetConfiguratedChannel<T>(ItauWCFConfiguration wcfServicesConfig,
            CertificateItem certificateConfig)
        {
            var binding = new BasicHttpsBinding
            {
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                Security =
                {
                    Mode = BasicHttpsSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                }
            };

            var certificationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? string.Empty, "Certificates",
                certificateConfig.Name);
            var certificate = new X509Certificate2(certificationPath, certificateConfig.Password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            var endpoint = new EndpointAddress(new Uri(wcfServicesConfig.EndpointUrl));

            var channelFactory = new ChannelFactory<T>(binding, endpoint);
            channelFactory.Credentials.ClientCertificate.Certificate = certificate;

            return channelFactory;
        }

        private Dictionary<string, dynamic> ProcessZipFromAttachmentLocal(string pathFile)
        {
            var ret = new Dictionary<string, dynamic>();

            try
            {
                var zipArchive = ZipFile.OpenRead(pathFile);
                var entry = zipArchive.Entries.Where(it =>
                    it.Name.StartsWith(ItauFilesConstants.PSFile) || it.Name.StartsWith(ItauFilesConstants.CVFile) || it.Name.StartsWith(ItauFilesConstants.NOFile));

                foreach (var archivo in entry)
                {
                    try
                    {
                        using TextReader tr = new StreamReader(archivo.Open());

                        {
                            if (archivo.Name.StartsWith(ItauFilesConstants.PSFile))
                            {
                                var psPayments = new List<PsPaymentItau>();
                                var engine = new MasterDetailEngine<HeaderItau, PsRegistroCashIn>(r =>
                                    SelectorMasterOrDetail(r, ItauFilesConstants.HeaderPSToken, ItauFilesConstants.PSFile));

                                var res = engine.ReadStream(tr);

                                foreach (var group in res)
                                {
                                    foreach (var Detail in @group.Details)
                                    {
                                        Detail.FechaOperacion = DateTime.ParseExact(archivo.Name[22..30], "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                                        psPayments.Add(new PsPaymentItau { Registro = Detail, Header = @group.Master });
                                    }
                                }

                                ret.Add(ItauFilesConstants.PSFile, psPayments);
                                continue;
                            }

                            if (archivo.Name.StartsWith(ItauFilesConstants.NOFile))
                            {
                                var list = new List<PiItau>();
                                var engine = new MasterDetailEngine<HeaderItau, PiItau>(r =>
                                    SelectorMasterOrDetail(r, ItauFilesConstants.HeaderNOToken, ItauFilesConstants.NOFile));

                                var res = engine.ReadStream(tr);

                                foreach (var group in res)
                                {
                                    list.AddRange(@group.Details);
                                }

                                ret.Add(ItauFilesConstants.NOFile, list);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error al momento de procesar archivo: {fileName}", archivo.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error al intentar descomprimir el attach de ItauWS: {@e}", e);
            }

            return ret;
        }

        public void GetAndProcessPaymentFilesCVUOfItauLocal()
        {
            try
            {
                var attachments = ProcessZipFromAttachmentLocal(@"..\application.zip");

                if (attachments.Keys.Contains(ItauFilesConstants.PSFile))
                {
                    _processTransactionService.ProcessRegistroFiles((List<PsPaymentItau>)attachments[ItauFilesConstants.PSFile]);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ERROR CVU");
            }
        }

        public void GetAndProcessPaymentFilesECHEQOfItauLocal()
        {
            try
            {
                var attachments = ProcessZipFromAttachmentLocal(@"..\application.zip");

                if (attachments.Keys.Contains(ItauFilesConstants.NOFile))
                {
                    var payments = new List<PaymentItau>();
                    var instruments = (List<PiItau>)attachments[ItauFilesConstants.NOFile];
                    var idPagos = instruments.Select(x => x.IdOperacion.Trim()).Distinct();

                    foreach (var idPago in idPagos)
                    {
                        payments.Add(new PaymentItau
                        {
                            Instruments = instruments.Where(x => x.IdOperacion == idPago).ToList()
                        });
                    }
                    _processTransactionService.ProcessRegistroFiles(payments);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ERROR ECHEQ");
            }
        }
    }
}
