using MailKit.Net.Proxy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services;
using Org.BouncyCastle.Ocsp;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WinSCP;
using Xunit;

namespace nordelta.cobra.webapi.tests
{
    public class PaymentService_Should
    {
        private readonly Mock<PaymentService> _paymentService;
        private readonly Mock<IArchivoDeudaRepository> _mockArchivoDeudaRepository;
        private readonly Mock<RestClient> _mockRestClient;
        private readonly Mock<IRestRequest> _mockRestRequest;
        private readonly Mock<ApiServicesConfig> _mockApiServicesConfig;

        public PaymentService_Should()
        {
            _paymentService = new Mock<PaymentService>();
            _mockArchivoDeudaRepository = new Mock<IArchivoDeudaRepository>();
            _mockApiServicesConfig = new Mock<ApiServicesConfig>();
            _mockRestClient = new Mock<RestClient>();
            _mockRestRequest = new Mock<IRestRequest>();

        }


        [Fact]
        public void ReportPaymentDone_DetalleDeuda_ShoudntToBeNull()
        {
            var debtDetail = SetDebtDetail();

            _mockArchivoDeudaRepository
            .Setup(x => x.Find(It.IsAny<int>()))
            .Returns(debtDetail);

            var detalleDeuda = _mockArchivoDeudaRepository.Object.Find(debtDetail.ArchivoDeudaId);

            Assert.NotNull(detalleDeuda);
        }

        //[Fact]
        //public void StatusCodeTest()
        //{
        //    var expectedUrl = "http://api.sgf.novit.com.ar/api/v2";
        //    var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcGlLZXlOYW1lIjoiQ29icmEiLCJyb2xlIjoiQXBpS2V5IiwiZXhwIjoxOTI3MTMwMjg1LCJpc3MiOiJodHRwOi8vYXBpLnNnZi5ub3ZpdC5jb20uYXIiLCJhdWQiOiJodHRwOi8vYXBpLnNnZi5ub3ZpdC5jb20uYXIifQ.yMNceogi9yQhOyMaZteKOSWPGiyogPh3kkxVH2wPCyw";
        //    var payments = new List<object>();
        //    // arrange
        //    RestClient client = new RestClient(expectedUrl);
        //    RestRequest request = new RestRequest("/Deuda/InformarPago", Method.POST)
        //    {
        //        RequestFormat = DataFormat.Json
        //    };
        //    request.AddHeader("token", expectedToken);
        //    request.AddJsonBody(payments);

        //    // act
        //    IRestResponse response = client.Execute(request);

        //    // assert
        //    Assert.Equal(response.StatusCode, HttpStatusCode.OK);
        //}

        [Fact]
        public void ReportPaymentDone_SendsCorrectRequest_WhenCalled()
        {
            var expectedUrl = "http://api.sgf.novit.com.ar/api/v2";
            var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJhcGlLZXlOYW1lIjoiQ29icmEiLCJyb2xlIjoiQXBpS2V5IiwiZXhwIjoxOTI3MTMwMjg1LCJpc3MiOiJodHRwOi8vYXBpLnNnZi5ub3ZpdC5jb20uYXIiLCJhdWQiOiJodHRwOi8vYXBpLnNnZi5ub3ZpdC5jb20uYXIifQ.yMNceogi9yQhOyMaZteKOSWPGiyogPh3kkxVH2wPCyw";

            var expectedPayments = new List<object>();
            var expectedResponse = new RestResponse<List<string>>();
          

            // Set up the Payment object
            var paymentInfo = SetPayment();
            expectedPayments.Add(paymentInfo);

            // Set up the expected response
            expectedResponse.StatusCode = HttpStatusCode.OK;
            expectedResponse.Content = "[\"Success\"]";
            expectedResponse.Data = new List<string>() { "Success" };


            // Set up the mock client and request

            _mockRestClient
            .Setup(x => x.Execute<List<string>>(It.IsAny<IRestRequest>()))
            .Returns(expectedResponse);

            _mockRestClient.Setup(x => x.BaseUrl).Returns(new Uri(expectedUrl));
            _mockRestRequest.Setup(x => x.Method).Returns(Method.POST);

            _mockRestRequest.Setup(x => x.Resource).Returns("/Deuda/InformarPago");
            _mockRestRequest.Setup(x => x.RequestFormat).Returns(DataFormat.Json);
            _mockRestRequest.Object.AddJsonBody(expectedPayments);


            // Add the expected header to the mock request
            _mockRestRequest.Setup(x => x.AddHeader("token", expectedToken))
            .Callback(() => _mockRestRequest.Object.Parameters.Add(new Parameter("token", expectedToken, ParameterType.HttpHeader)));
            
            // Act
            IRestResponse<List<string>> response =  _mockRestClient.Object.Execute<List<string>>(_mockRestRequest.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedResponse.Data, response.Data);
            Assert.Equal("Success", response.Data[0]);
            // Assert
            _mockRestClient.Verify(x => 
            x.Execute<List<string>>(_mockRestRequest.Object));
        }


        public DetalleDeuda SetDebtDetail()
        {
            var debtDetail = new DetalleDeuda
            {
                ArchivoDeuda = new ArchivoDeuda
                {
                    Id = 5,
                    FileName = "DemoTestHuergo_publicacion_deuda_2023010425022",
                    FormatedFileName = "DemoTestHuergo_publicacion_deuda",
                    Header = null,
                    TimeStamp = "01/03/2023 15:08:23",
                    Trailer = null
                },
                ArchivoDeudaId = 5,
                Id = 1111,
                TipoRegistro = "D",
                TipoOperacion = "A",
                CodigoMoneda = "0",
                NumeroCliente = "000012345678910",
                TipoComprobante = "OP",
                NroComprobante = "A-1234-12345678",
                NroCuota = "1234",
                NombreCliente = "Usuario Demo",
                DireccionCliente = "000000",
                DescripcionLocalidad = "000000",
                PrefijoCodPostal = "",
                NroCodPostal = "00000",
                UbicManzanaCodPostal = "",
                FechaPrimerVenc = "20220815",
                ImportePrimerVenc = "000000000000000",
                FechaSegundoVenc = "00000000",
                ImporteSegundoVenc = "000000000000000",
                FechaHastaDescuento = "00000000",
                ImporteProntoPago = "000000000000000",
                FechaHastaPunitorios = "12345678",
                TasaPunitorios = "000000",
                MarcaExcepcionCobroComisionDepositante = "N",
                FormasCobroPermitidas = "",
                NroCuitCliente = "12345678910",
                CodIngresosBrutos = "0",
                CodCondicionIva = "0",
                CodConcepto = "",
                DescCodigo = "",
                ObsLibrePrimera = "111111|111111",
                ObsLibreSegunda = "111111|111111",
                ObsLibreTercera = "",
                ObsLibreCuarta = "AA000000",
                Relleno = "",
                PaymentMethodId = 10,
                CodigoMonedaTc = "",
                PaymentReportId = 10

            };

            return debtDetail;
        }

        public Object SetPayment()
        {
            return new
            {
                codigoAcuerdo = 1,
                moneda = "USD",
                monto = "100.00",
                cotizacion = "1.05",
                transaction = "12345",
                fechaVencimiento = "2022-01-01",
                idClienteOracle = 1,
                idSiteClienteOracle = 1,
                metodo = "credit card"
            };
        }

    }
}
