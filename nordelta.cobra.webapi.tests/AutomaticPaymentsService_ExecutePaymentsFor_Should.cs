using nordelta.cobra.webapi.Services;
using System;
using Xunit;
using Moq;
using nordelta.cobra.webapi.Repositories;
using nordelta.cobra.webapi.Services.Mocks;
using nordelta.cobra.webapi.Services.Contracts;
using System.Collections.Generic;
using System.Linq;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using Microsoft.Extensions.Configuration;
using nordelta.cobra.webapi.Utils;
using Hangfire;

namespace nordelta.cobra.webapi.tests
{
    public class AutomaticPaymentsService_ExecutePaymentsFor_Should
    {
        public AutomaticPaymentsService_ExecutePaymentsFor_Should() { }

        // TODO: Agregar casos de test cuando este funcionando Multiples Pagos parciales
        // - varias cuotas a vencer, 1 vencida y con pago parcial
        // - 1 cuota a vencer, con pago parcial. 000000000492149

        //[Theory]
        // Carpinchos  284 - Solo 1 cuota a vencer de 1 producto, simple. No existen cuotas anteriore vencidas
        //[InlineData(2019, 08, 02, 1, "0170116240000008359999", 4921.49)]

        // Virazon     319 - Vence esta y la anteriorque ya estaba vencida tmb
        //[InlineData(2019, 08, 08, 2, "0170116240000008359999,0170071840000032279999", 11480.58)]

        // Carpinchos   13 - Paga solo 1 cuota, las 2 anteriores estan pagas.
        // El Yacht    201 - No paga nada, tiene todas las vencidas pagas (incluso la de fecha), las posteriores no
        // GOLF        217 - Paga 3 cuotas, la actual y las 2 anteriores impagas.
        // Tambien generara la de Virazo  319, de Carpinchos 284 ya que esta cargada en automaticdeins, y ya esta vencida
        // Y tambien va a generar de Golf 216 porque aca ya tiene que tner alguna cuota vencida.
        //[InlineData(2019, 10, 15, 5, "0170116240000008359999,0170071840000032279999,1500048000009063039999,0270001420000056189999,0170166740000003939999", 56709.77)]

        // El Golf     216 - La que vence ese dia es la ultima cuota, y estan todas las anteriores impagas. Tiene que pagar todo el producto completo.
        //                   Este producto tambien tiene deuda publicada en diferentes archivos de publicacion, por ende solo tiene que tomar el vigente.
        // El Yacht    201 - Aca si se paga, y este producto esta cargado en DeginAutomatico en pesos.
        // Tambien generara todos los anteriores.
        //[InlineData(2021, 01, 15, 6, "0170116240000008359999,0170071840000032279999,1500048000009063039999,0270001420000056189999,0170166740000003939999,0720130788000022579999", 875267.30)]
        //public void PayDebtsDueParamDate(int year, int month, int day, int assertCantDebinGenerados, string assertCbuUtilizados, double assertTotalDineroCobrado)
        //{
        //    var connection = TestDBHelper.GetOpenedConnection();
        //    var context = TestDBHelper.GetPopulatedRelationalContext(connection, "CobraTestAutomaticPaymentsDBData.sql");

        //    try
        //    {
        //        var automaticDebinRepository = new AutomaticDebinRepository(context);
        //        var archivoDeudaRepository = new ArchivoDeudaRepository(context);
        //        var bankAccountRepository = new BankAccountRepository(context);
        //        var anonymousPaymentRepository = new AnonymousPaymentRepository(context);
        //        var debinRepository = new DebinRepository(context);
        //        var companyRepository = new CompanyRepository(context);
        //        var restClient = new RestSharp.RestClient();

        //        //Es PartialMock, porqe es la instancia original excepto InformPaymentDone que no hace nada.
        //        var emailServiceMock = new Mock<IMailService>(0);
        //        var paymentsServicePartialMock = new Mock<PaymentService>(restClient, null, archivoDeudaRepository, null, null);
        //        var configurationMock = new Mock<IConfiguration>();
        //        var configurationSectionMock = new Mock<IConfigurationSection>();
        //        var backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        //        configurationSectionMock.Setup(x => x.Value).Returns("120");
        //        configurationMock.Setup(x => x.GetSection(It.IsAny<string>())).Returns(configurationSectionMock.Object);
        //        paymentsServicePartialMock.CallBase = true;//para usar la instancia real
        //        paymentsServicePartialMock.Setup(x =>
        //        x.InformPaymentDone(It.IsAny<IOrderedEnumerable<DetalleDeuda>>())
        //        ).Verifiable();

        //        var loginServiceMock = new Mock<ILoginService>();
        //        loginServiceMock.Setup(x => x.GetUserById(It.IsAny<string>()))
        //            .Returns<string>(userId =>
        //            {
        //                List<string> cuits;
        //                switch (userId)
        //                {
        //                    case "TESTPAYER_1":
        //                        cuits = new List<string>() { "20243134294", "cuitfalsoparaversirompe" };
        //                        break;
        //                    case "TESTPAYER_2":
        //                        cuits = new List<string>() { "20264074666" };
        //                        break;
        //                    case "TESTPAYER_3":
        //                        cuits = new List<string>() { "20337619127" };
        //                        break;
        //                    case "TESTPAYER_4":
        //                        cuits = new List<string>() { "27264208497" };
        //                        break;
        //                    case "TESTPAYER_5":
        //                        cuits = new List<string>() { "27927525408", "cuitfalsoparaversirompe" };
        //                        break;
        //                    case "TESTPAYER_6":
        //                        cuits = new List<string>() { "30713954205" };
        //                        break;
        //                    default: return null;
        //                }
        //                return new User()
        //                {
        //                    Id = userId,
        //                    Cuit = Convert.ToInt64(cuits.First()),
        //                    AdditionalCuits = cuits

        //                };
        //            });


        //        var itauServiceMock = new ItauServiceMock(configurationMock.Object, emailServiceMock.Object);

        //        var debinService = new DebinService(
        //            paymentsServicePartialMock.Object,
        //            debinRepository,
        //            companyRepository,
        //            bankAccountRepository,
        //            archivoDeudaRepository,
        //            configurationMock.Object,
        //            anonymousPaymentRepository,
        //            itauServiceMock,
        //            null,
        //            backgroundJobClientMock.Object
        //        );
           
        //        var _automaticPaymentsService = new AutomaticPaymentsService(
        //            automaticDebinRepository,
        //                archivoDeudaRepository,
        //                debinService,
        //                paymentsServicePartialMock.Object,
        //                loginServiceMock.Object,
        //                null
        //            );

        //        var date = new DateTime(year, month, day).Date;
        //        //Ejecuta metodo a testear
        //        _automaticPaymentsService.ExecutePaymentsFor(date);

        //        //Verifica los cambios que deberia haber realizado
        //        var debinGenerados = context.Debin.ToList(); //Consulta a la db
        //        debinGenerados = debinGenerados.Where(x => (LocalDateTime.GetDateTimeNow() - x.IssueDate).TotalMinutes <= 10).ToList(); // realiza la query en memoria con los resultados
        //        Assert.Equal(assertCantDebinGenerados, debinGenerados.Count);

        //        double totalCobrado = Math.Round(debinGenerados.Select(x => x.Amount).Aggregate((a, b) => a + b), 2);
        //        Assert.Equal(assertTotalDineroCobrado, totalCobrado);

        //        string[] usedCbus = debinGenerados.Select(x => x.BankAccount.Cbu).ToArray();
        //        string[] assertCbus = assertCbuUtilizados.Split(",");

        //        Assert.True(assertCbus.All(x => usedCbus.Contains(x)) && usedCbus.Length == assertCbus.Length);


        //    }
        //    finally
        //    {
        //        context.Dispose();
        //        connection.Close();
        //    }
        //}
    }
}
