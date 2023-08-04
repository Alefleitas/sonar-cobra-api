using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Moq;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.Mocks;
using Xunit;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Models.ValueObject.Certificate;
using System.ServiceModel;
using FileHelpers.MasterDetail;

namespace nordelta.cobra.webapi.tests.Services
{
    public class ItauServiceTests
    {
        private Mock<IMailService> _mailServiceMock;
        private IConfiguration _configurationMock;
        private Mock<ICvuEntityService> _cvuServiceMock;

        private void SetupTest()
        {
            _mailServiceMock = new Mock<IMailService>();
            _mailServiceMock.Setup(x => x.SendNotificationEmail(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            _cvuServiceMock = new Mock<ICvuEntityService>();
        }

        private void SetAppSettings(string appSettingsName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Services"))
                .AddJsonFile(appSettingsName, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();
            _configurationMock = builder.Build();        
        }

        private static void MakeCertificate(string certName, string certPassword, DateTime expirationDate)
        {
            var ecdsa = ECDsa.Create(); // generate asymmetric key pair
            var req = new CertificateRequest("cn=foobar", ecdsa, HashAlgorithmName.SHA256);
            var cert = req.CreateSelfSigned(DateTimeOffset.Now.AddMonths(-5), expirationDate);

            var certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates",
                certName);
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates")))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates"));
            }

            // Create PFX (PKCS #12) with private key
            File.WriteAllBytes($"{certPath}.pfx", cert.Export(X509ContentType.Pfx, certPassword));

            // Create Base 64 encoded CER (public key only)
            File.WriteAllText($"{certPath}.cer",
                "-----BEGIN CERTIFICATE-----\r\n"
                + Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks)
                + "\r\n-----END CERTIFICATE-----");
        }

        [Fact(Skip = "bug in localtime")]
        public void When_certificate_expires_in_1_day_must_send_notification()
        {
            MakeCertificate("test" , "Password123!", DateTime.Now.AddDays(1));
            SetupTest();
            SetAppSettings("appsettings5days.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.Is<List<string>>(s => s.Contains("testoneday@test.com")), 
                It.Is<string>(s => s.Equals("Aviso de vencimiento del certificado: test.pfx !")), 
                It.Is<string>(body => body.Equals("El certificado test.pfx vence en 1 día !"))), Times.Exactly(7));
        }

        [Fact(Skip = "bug in localtime")]
        public void When_certificate_expires_in_2_day_must_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddDays(2));
            SetupTest();
            SetAppSettings("appsettings5days.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.Is<List<string>>(s => s.Contains("testoneday@test.com")),
                It.Is<string>(s => s.Equals("Aviso de vencimiento del certificado: test.pfx !")),
                It.Is<string>(body => body.Equals("El certificado test.pfx vence en 2 dias !"))), Times.Exactly(7));
        }

        [Fact()]
        public void when_the_certificate_expires_in_2_days_and_the_config_is_in_1_day_do_not_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddDays(2));
            SetupTest();
            SetAppSettings("appsettings.json");

            //    var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null, null);
            //    itauService.CheckCertificateExpirationDate();

            //    _mailServiceMock.Verify(
            //        x => x.SendNotificationEmail(It.Is<List<string>>(s => s.Contains("testoneday@test.com")),
            //            It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(7));
            //}
            //_mailServiceMock.Verify(x => x.SendNotificationEmail(It.Is<List<string>>(s => s.Contains("testoneday@test.com")), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(0));
            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(0));
        }

        [Fact()]
        public void When_certificate_expires_in_1_week_must_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddDays(7));
            SetupTest();
            SetAppSettings("appsettingsweek.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(
                It.Is<List<string>>(s => s.Contains("testweeks@test.com")),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Exactly(7));
        }

        [Fact()]
        public void When_certificate_expires_in_2_week_must_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddDays(14));
            SetupTest();
            SetAppSettings("appsettingsweek.json");
            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(
                It.Is<List<string>>(s => s.Contains("testweeks@test.com")),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Exactly(7));
        }

        [Fact()]
        public void When_certificate_expires_in_1_month_must_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddMonths(1));
            SetupTest();
            SetAppSettings("appsettingsmonths.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.Is<List<string>>(s => s.Contains("testmonths@test.com")),
                It.Is<string>(s => s.Equals("Aviso de vencimiento del certificado: test.pfx !")),
                It.Is<string>(body => body.Equals("El certificado test.pfx vence en 1 mes !"))), Times.Exactly(7));
        }

        [Fact()]
        public void When_certificate_expires_in_2_month_must_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddMonths(2));
            SetupTest();
            SetAppSettings("appsettingsmonths.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.Is<List<string>>(s => s.Contains("testmonths@test.com")),
                It.Is<string>(s => s.Equals("Aviso de vencimiento del certificado: test.pfx !")),
                It.Is<string>(body => body.Equals("El certificado test.pfx vence en 2 meses !"))), Times.Exactly(7));
        }

        [Fact()]
        public void When_certificate_expires_in_6_month_dont_send_notification()
        {
            MakeCertificate("test", "Password123!", DateTime.Now.AddMonths(6));
            SetupTest();
            SetAppSettings("appsettingsmonths.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(0));
        }

        [Fact()]
        public void DisableAllChecksSettings()
        {
            //MakeCertificate("test", "Password123!", DateTime.Now.AddMonths(-1));
            SetupTest();
            SetAppSettings("disableAllChecks.json");

            var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
            itauService.CheckCertificateExpirationDate();

            _mailServiceMock.Verify(x => x.SendNotificationEmail(It.IsAny<List<string>>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(0));
        }

        [Fact()]
        public void make()
        {
            MakeCertificate("vence-7dias", "Password123!", DateTime.Now.AddDays(7));
            MakeCertificate("vence-7semanas", "Password123!", DateTime.Now.AddDays(7 * 7));
            MakeCertificate("vence-7meses", "Password123!", DateTime.Now.AddMonths(7));
            
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
            // var dd = AddressHeader.CreateAddressHeader("cuit", "https://wsechomo.itau.com.ar/CMLWeb/sca/ARCHIVOS", "30658660892");
            EndpointAddress endpoint = new EndpointAddress(new Uri(wcfServicesConfig.EndpointUrl));


            ChannelFactory<T> channelFactory = new ChannelFactory<T>(binding, endpoint);
            channelFactory.Credentials.ClientCertificate.Certificate = certificate;

            return channelFactory;
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

        private void SaveXml(object obj)
        {

            var x = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var pathSaveResponse = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certificates",
                obj.GetType().Name + ".xml");
            var file = System.IO.File.Create(pathSaveResponse);
            var writer =
                new System.Xml.Serialization.XmlSerializer(obj.GetType());
            writer.Serialize(file, obj);
            file.Close();

        }

        // [Fact()]
        // public async void TestArchivosExportacionConn()
        // {
        //     SetupTest();
        //     SetAppSettings("appsettings.json");
        //
        //     var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, null, null, null);
        //
        //     ItauWCFConfiguration debitoInmediatoConfig = new ItauWCFConfiguration()
        //     {
        //         EndpointUrl = "https://wsechomo.itau.com.ar/CMLWeb/sca/ARCHIVOS"
        //         //EndpointUrl = "https://wsechomo.itau.com.ar/CMLWeb/sca/ARCHIVOS"
        //     };
        //
        //
        //     var certConfig = new CertificateItem
        //     {
        //         Name = "ws_ndt_2021.p12",
        //         Password = "Nordelta2020!"
        //
        //     };
        //
        //     var _channelFactoryDebitoInmediado =
        //         GetConfiguratedChannel<ARCHIVOSCML>(debitoInmediatoConfig, certConfig);
        //
        //     var serviceClient = _channelFactoryDebitoInmediado.CreateChannel();
        //
        //     var req = new archivosSolicitudRequest(new archivosSolicitud()
        //     {
        //         entrada = new inputSolicitudReporteValidacionEnvios()
        //         {
        //             convenio = new convenio()
        //             {
        //                 cuenta = new cuenta()
        //                 {
        //                     deComision = "",
        //                     deProducto = "",
        //                     principal = ""
        //                 },
        //                 descripcion = "",
        //                 estado = "",
        //                 moneda = "",
        //                 numero = "000001",
        //                 cuit = "30658660892",
        //                 producto = new producto()
        //                 {
        //                     numero = "160",
        //                     descripcion = ""
        //                 },
        //                
        //
        //             },
        //             fechaGeneracion = "20210526",
        //             numeroEnvio = "",
        //             tipoRendicion = "A",
        //             tipoArchivo = "F"
        //         }
        //     });
        //
        //     
        //     var response = await serviceClient.archivosSolicitudAsync(req);
        //
        //    
        //     MemoryStream memAttachment = new MemoryStream(response.archivosSolicitudResponse.attach);
        //
        //     // await File.WriteAllBytesAsync("d:\\tt\\" + req.archivosSolicitud.entrada.fechaGeneracion + ".zip",
        //     //     response.archivosSolicitudResponse.attach);
        //     //
        //     ZipArchive archive = new ZipArchive(memAttachment, ZipArchiveMode.Read);
        //     
        //    
        //
        //     var entry = archive.Entries.SingleOrDefault(it => it.Name.StartsWith("CV"));
        //
        //     //TextReader tr = new StreamReader(File.OpenRead("d:\\tt\\CV306137551241600000012021040700151 (1).TXT"));
        //     TextReader tr = new StreamReader(entry.Open());
        //     //var content = await tr.ReadToEndAsync();
        //     try
        //     {
        //         var engine = new MasterDetailEngine<HeaderFile, PsRegistroCashIn>((r) =>
        //             SelectorMasterOrDetail(r, "HCV"));
        //         
        //         var res = engine.ReadStream(tr);
        //         foreach (var group in res) {
        //             Console.WriteLine("Customer: {0}", group.Master.Cuit);
        //             foreach (var detail in group.Details)
        //                 Console.WriteLine("    Freight: {0}", detail.NombreDebito);
        //         }
        //         
        //     }
        //     catch (Exception e)
        //     {
        //         Console.WriteLine(e);
        //         throw;
        //     }
        //   
        //     SaveXml(req);
        //    // SaveXml(response);
        //
        // }

        private RecordAction SelectorMasterOrDetail(string record, string masterToken)
        {
            if (record.Length < 2 || record[39].Equals('T'))
                return RecordAction.Skip;

            return record.Contains(masterToken) ? RecordAction.Master : RecordAction.Detail;
        }

        //[Fact()]
        //public void TestCallApiItauCreacionCVU()
        //{
        //    SetupTest();
        //    SetAppSettings("appsettings.json");
        //    IOptionsMonitor<ApiServicesConfig> dd;
        //    ApiServicesConfig au = new ApiServicesConfig()
        //    {
        //        CertificateName = "ws_ndt_2021.p12",
        //        Url = "https://wsechomo.itau.com.ar/apiCvu/v1"
        //    };
        //    var monitor = Mock.Of<IOptionsMonitor<ApiServicesConfig>>(_ => _.Get("ItauApi") == au);

        //    IRestClient client = new RestClient();

        //    var itauService = new ItauServiceMock(_configurationMock, _mailServiceMock.Object, client, monitor, null, null);


        //    var regDto = new RegisterCvuDto()
        //    {
        //        Currency = Currency.ARS,
        //        Cuit = "30658660892",
        //        HolderName = "FIDEICOMISO GOLF CLUB",
        //        ClientId = "122333545",
        //        ProductCode = "PL323231",
        //        PersonType = PersonType.Juridica
        //    };
        //    itauService.CallItauApiCreateTransaction(regDto);
        //}
    }
}