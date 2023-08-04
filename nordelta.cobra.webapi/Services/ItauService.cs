using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using DebitoInmediatoServiceItau;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Models;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using Serilog;
using nordelta.cobra.webapi.Utils;
using nordelta.cobra.webapi.Extensions;
using nordelta.cobra.webapi.Models.ValueObject.ItauPsp;
using RestSharp;
using ArchivosCmlServiceItau;

namespace nordelta.cobra.webapi.Services
{
    public class ItauService : IItauService
    {
        private readonly IOptionsMonitor<ItauWCFConfiguration> _itauConfiguration;
        //private readonly IOptionsMonitor<ItauCertificateConfig> _itauCertConfiguration;
        private ChannelFactory<DebitoInmediato> _channelFactoryDebitoInmediado;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly List<CertificateItem> _certificateItems;
        private readonly IRestClient _restClient;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;

        public ItauService(
            //IOptionsMonitor<ItauCertificateConfig> itauCertConfiguration,       
            IConfiguration configuration,
            IMailService mailService,
            IRestClient restClient,
            IOptionsMonitor<ApiServicesConfig> apiServicesConfig,
            IOptionsMonitor<ItauWCFConfiguration> options)
        {
            _itauConfiguration = options;
            //_itauCertConfiguration = itauCertConfiguration;
            _configuration = configuration;
            _mailService = mailService;
            _certificateItems = _configuration.GetSection("ServiceConfiguration:CertificateSettings:CertificateItems").Get<List<CertificateItem>>();
            foreach (var certificateItem in _certificateItems)
                certificateItem.Password = AesManager.GetPassword(certificateItem.Password, _configuration.GetSection("SecretKeyCertificate").Value);
            _restClient = restClient;
            _apiServicesConfig = apiServicesConfig;
        }

        public async Task<RegistrarPublicacionResponse> RegisterPublicationAsync(RegisterPublicationDTO debinData, bool isAnonymousPayment = false)
        {
            ItauWCFConfiguration debitoInmediatoConfig =
                    _itauConfiguration.Get(ItauWCFConfiguration.DebitoInmediatoConfiguration);

            var certConfig = GetItauCertificateConfig(debinData.Vendedor.Cuit);

            _channelFactoryDebitoInmediado =
                GetConfiguratedChannel<DebitoInmediato>(debitoInmediatoConfig, certConfig);

            DebitoInmediato serviceClient = _channelFactoryDebitoInmediado.CreateChannel();

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
                            codigo = debinData.CompradorCuit,
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

            try
            {
                var result = await serviceClient.RegistrarPublicacionAsync(request);

                _channelFactoryDebitoInmediado.Close();

                if (result.header.transaccion.estado.Equals("Exito"))
                {
                    Log.Debug(@"DebinSuccess, 
                            Type: PublishDebinSuccess,
                            Description: Debin Published successful:
                            request: {@request},
                            response: {response}", request, result.header.transaccion.estado);
                    return result;
                }
                else
                {
                    Log.Debug(@"DebinFailed, 
                            Type: PublishDebinFailed,
                            Description: Failed to publish debin:
                            request: {@request},
                            response: {response}", request, result.header.transaccion.estado);
                    throw new Exception("Error al publicar el Debin: " + JsonConvert.SerializeObject(result));
                }
            }
            catch (Exception e)
            {
                Log.Error(@"DebinError, 
                            Type: PublishDebinError,
                            Description: Debin Publish error:
                            request: {@request},
                            response: {@error}", request, e);
                throw new Exception("Excepción al publicar el Debin: " + JsonConvert.SerializeObject(e));
            }

        }

        public async Task<PaymentStatus> GetDebinState(GetDebinStateRequest debinStateRequest)
        {
            try
            {
                ItauWCFConfiguration debitoInmediatoConfig =
                    _itauConfiguration.Get(ItauWCFConfiguration.DebitoInmediatoConfiguration);

                var certConfig = GetItauCertificateConfig(debinStateRequest.Cuit);

                _channelFactoryDebitoInmediado =
                    GetConfiguratedChannel<DebitoInmediato>(debitoInmediatoConfig, certConfig);

                DebitoInmediato serviceClient = _channelFactoryDebitoInmediado.CreateChannel();
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


                string state = "ERROR";

                try
                {
                    ConsultarEstadoResponse result = await serviceClient.ConsultarEstadoAsync(request);
                    if (result.header.transaccion.estado.Equals("Exito"))
                    {
                        state = ((ConsultarEstadoResponseType)result.body.Item).descripcion;
                        Log.Debug(@"GetDebinState, 
                            Type: GetDebinStateSuccess,
                            Description: Get Debin State successful:
                            request: {@request},
                            response: {response}", request, result.header.transaccion.estado);
                    }
                    else
                    {
                        Log.Debug(@"GetDebinState, 
                            Type: GetDebinStateFailed,
                            Description: Failed Get Debin State:
                            request: {@request},
                            response: {response}", request, result.header.transaccion.estado);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(@"GetDebinState, 
                            Type: GetDebinStateError,
                            Description: Error on Get Debin State:
                            request: {@request},
                            response: {@error}", request, e);
                    throw new Exception("Excepción al publicar el Debin: " + JsonConvert.SerializeObject(e));
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

                _channelFactoryDebitoInmediado.Close();

                return currentStatus;
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in GetDebinState.");
                throw;
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
                        body = body.Replace("{amount}", daysDiff + (daysDiff > 1 ? " dias" : " día"));

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

        private CertificateItem GetItauCertificateConfig(string vendorCuit)
        {
            return _certificateItems.SingleOrDefault(it => it.VendorCuit == vendorCuit);
        }

        private static ChannelFactory<T> GetConfiguratedChannel<T>(ItauWCFConfiguration wcfServicesConfig,
            CertificateItem certificateConfig)
        {

            BasicHttpsBinding binding = new BasicHttpsBinding
            {
                Security =
                {
                    Mode = BasicHttpsSecurityMode.Transport,
                    Transport = {ClientCredentialType = HttpClientCredentialType.Certificate}
                }
            };

            string certificationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates",
                certificateConfig.Name);
            X509Certificate2 certificate = new X509Certificate2(certificationPath, certificateConfig.Password,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);

            EndpointAddress endpoint = new EndpointAddress(new Uri(wcfServicesConfig.EndpointUrl));

            ChannelFactory<T> channelFactory = new ChannelFactory<T>(binding, endpoint);
            channelFactory.Credentials.ClientCertificate.Certificate = certificate;

            return channelFactory;
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
                    $"Error: CallItauApiCreateTransaction, registerCvuDto: {JsonConvert.SerializeObject(registerCvuDto)} \n " +
                    $"request: {JsonConvert.SerializeObject(request)} \n" +
                    $"response: {JsonConvert.SerializeObject(response)}");

            Log.Information("CallItauApiCreateTransaction a {@url} iniciado \n request: {@request}", request.Resource,
                request);
            Log.Debug("CallItauApiCreateTransaction to Itau \n request: {@request} \n response:{@response}", request,
                response);
            return response.Data;
        }

    }
}
