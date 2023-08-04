using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class AutomaticPaymentsService : IAutomaticPaymentsService
    {
        IAutomaticDebinRepository _automaticDebinRepository;
        IDebinService _debinService;
        IArchivoDeudaRepository _archivoDeudaRepository;
        IPaymentService _paymentService;
        ILoginService _loginService;
        IConfiguration _configuration;

        public AutomaticPaymentsService(IAutomaticDebinRepository automaticDebinRepository,
            IArchivoDeudaRepository archivoDeudaRepository,
            IDebinService debinService,
            IPaymentService paymentService,
            ILoginService loginService,
            IConfiguration configuration)
        {
            this._automaticDebinRepository = automaticDebinRepository;
            this._debinService = debinService;
            this._archivoDeudaRepository = archivoDeudaRepository;
            this._paymentService = paymentService;
            this._loginService = loginService;
            this._configuration = configuration;
        }


        public void ExecutePaymentsFor(DateTime date)
        {

            Log.Debug("Ejecutando pagos automaticos a la fecha {date}", date);
            int automaticDebinExpirationTimeInMinutes = _configuration != null
                ? Convert.ToInt32(_configuration.GetSection("AutomaticDebinDueTimeInMinutes").Value)
                : 600;

            List<AutomaticPayment> automaticPayments = _automaticDebinRepository.All();
            foreach (var automaticPayment in automaticPayments)
            {
                try
                {
                    Log.Debug("Ejecutando pagos automaticos para propiedad: {propiedad} a la fecha {date}", automaticPayment.Product, date.Date);

                    if (automaticPayment.Currency != automaticPayment.BankAccount.Currency)
                        throw new Exception("Currency mismatch between AutomaticPayment & BankAccount");

                    if (automaticPayment.Payer?.Email == null)
                    {
                        automaticPayment.Payer = this._loginService.GetUserById(automaticPayment.Payer.Id);
                    }


                    var paymentsDueToday = GetDuePaymentsUntil(date.Date, automaticPayment);
                    Log.Debug("Se encontraron {count} cuotas vencidas en {currency} para {product} a la fecha {date}", paymentsDueToday.Count, automaticPayment.Currency.ToString(), automaticPayment.Product, date.Date);
                    if (paymentsDueToday != null && paymentsDueToday.Count > 0)
                    {
                        var publishDebin = new PublishDebinViewModel();

                        publishDebin.CompradorCbu = automaticPayment.BankAccount.Cbu;

                        publishDebin.DebtAmounts = paymentsDueToday
                            .Select(x => new DebtViewModel() { DebtId = x.Id, Amount = (Convert.ToDouble(x.ImportePrimerVenc) / 100) })
                            .ToList();

                        publishDebin.Importe =
                            Math.Round(
                                 paymentsDueToday
                                 .Select(x => (Convert.ToDouble(x.ImportePrimerVenc) / 100)) //Divide por 100 porque los 2 ultimos caracteres son decimales
                                 .Aggregate((a, b) => a + b),
                                 2
                            );

                        publishDebin.Moneda = automaticPayment.Currency;

                        publishDebin.Comprobante = automaticPayment.Product;

                        publishDebin.VendedorCuit = paymentsDueToday.First().ArchivoDeuda.Header.Organismo.CuitEmpresa;

                        _debinService.PublishDebin(publishDebin, automaticPayment.Payer, automaticDebinExpirationTimeInMinutes).Wait();
                        Log.Information("Se genero DEBIN automatico: {@debin}, para la propiedad: {propiedad}, del usuario: {user}", publishDebin, automaticPayment.Product, automaticPayment.Payer.Email);
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex, "No se pudo generar los DEBIN para el DebinAutomatico: {@automaticPayment}. UntilDate:{date}", automaticPayment, date);
                }
            }

        }

        private List<DetalleDeuda> GetDuePaymentsUntil(DateTime date, AutomaticPayment automaticPayment)
        {
            // WARNING: 
            // usa x.Debin == null en vez de GetRemainingAmountToPayForDebt
            // porque el modelo de BD solo permite un DEBIN por DetalleDeuda (no permite N pagos)
            // Por lo cual solo podemos generar DEBIN para cuotas sin un debin generado.
            // Esto va a cambiar cuando se permitan multiples pagos parciales.
            // Ahi hay que usar el metodo

            List<DetalleDeuda> result;
            if (automaticPayment.Payer.AdditionalCuits != null && automaticPayment.Payer.AdditionalCuits.Any())
            {
                result = _paymentService.GetAllPayments(automaticPayment.Payer.AdditionalCuits);
            }
            else
            {
                result = _paymentService.GetAllPayments(automaticPayment.Payer.Cuit.ToString());
            }
            result = result
                .Where(x => x.NroComprobante == automaticPayment.Product
                                  && Convert.ToInt32(x.CodigoMoneda) == (int)automaticPayment.Currency
                                  && DateTime.ParseExact(x.FechaPrimerVenc, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None) <= date // Agarra todo lo que vence hoy, o vencio antes pero no se pago.
                                  && x.PaymentMethod == null
                // && _paymentService.GetRemainingAmountToPayForDebt(x) > 0
                )
                .ToList();

            return result;
        }
    }
}
