using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Configuration;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Models.ArchivoDeuda;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Utils;
using RestSharp;
using Serilog;

namespace nordelta.cobra.webapi.Services
{
    public class PaymentReportsService : IPaymentReportsService
    {
        private readonly IPaymentReportRepository _paymentReportRepository;
        private readonly IArchivoDeudaRepository _archivoDeudaRepository;
        private readonly INotificationService _notificationService;
        private readonly IHolidaysService _holidaysService;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IOptionsMonitor<ApiServicesConfig> _apiServicesConfig;
        private readonly IRestClient _restClient;

        public PaymentReportsService(
            IPaymentReportRepository paymentReportRepository,
            IArchivoDeudaRepository archivoDeudaRepository,
            INotificationService notificationService,
            IHolidaysService holidaysService,
            IConfiguration configuration,
            IUserRepository userRepository,
            IOptionsMonitor<ApiServicesConfig> apiServicesConfig,
            IRestClient restClient
            )
        {
            _paymentReportRepository = paymentReportRepository;
            _archivoDeudaRepository = archivoDeudaRepository;
            _notificationService = notificationService;
            _holidaysService = holidaysService;
            _configuration = configuration;
            _userRepository = userRepository;
            _apiServicesConfig = apiServicesConfig;
            _restClient = restClient;
        }

        public PaymentReportCommandResponseDto CreatePaymentReport(PaymentReportDto report, string userId)
        {
            // Verificar que el cliente no tenga informes de pago activos
            if (_paymentReportRepository.GetSingle(x => x.PayerId == userId &&
                                                       x.Status == PaymentReportStatus.Created &&
                                                       x.ReportDateVto >= LocalDateTime.GetDateTimeNow()) is not null)
            {
                return new PaymentReportCommandResponseDto
                {
                    Message = "Posee un informe de pago previo, por favor complete el pago y vuelva a intentarlo"
                };
            }

            var debts = _archivoDeudaRepository.FindMany(report.DebtIds);
            var sameDebtsWithOtherCurrency = new List<DetalleDeuda>();

            try
            {
                foreach (var debt in debts)
                {
                    try
                    {
                        var debtWithOtherCurrency = _archivoDeudaRepository.FindSameDebtFromAnotherCurrency(debt);
                        if (debtWithOtherCurrency != null)
                        {
                            sameDebtsWithOtherCurrency.Add(debtWithOtherCurrency);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error en PaymentReportService al intentar obtener el detalle de deuda en otra moneda para la cuota con Id: {debt}.", debt.Id);
                    }
                }

                if (sameDebtsWithOtherCurrency.Count() != debts.Count())
                    return new PaymentReportCommandResponseDto
                    {
                        Message = "No se pudo guardar el informe de pago"
                    };

                List<DetalleDeuda> allDebts = debts.Concat(sameDebtsWithOtherCurrency).ToList();

                var businessDays = _configuration.GetSection("EcheqDueTimeInDays").Get<int>();
                var reportDateVto = LocalDateTime.GetDateTimeNow();

                while (businessDays > 0)
                {
                    reportDateVto = _holidaysService.GetNextWorkDayFromDate(reportDateVto.AddDays(1));
                    businessDays--;
                }

                var newReport = new PaymentReport
                {
                    PayerId = userId,
                    Debts = allDebts,
                    ReportDate = LocalDateTime.GetDateTimeNow(),
                    Cuit = report.Cuit,
                    Currency = report.Currency == 0 ? Currency.ARS : Currency.USD,
                    Amount = report.Amount,
                    Instrument = report.Type,
                    Product = report.Product,
                    ReportDateVto = reportDateVto
                };

                if (_paymentReportRepository.Insert(newReport))
                    return new PaymentReportCommandResponseDto
                    {
                        Message = $"Informe de pago para el producto {newReport.Product} guardado correctamente, " +
                        $"tiene hasta el {newReport.ReportDateVto} para pagarlo de lo contrario tendra que realizarlo nuevamente"
                    };

                throw new Exception("Error al guardar el informe de pago");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CreatePaymentReport: Ocurrió un error al intentar crear una paymentReport : {@report}", report);
                throw;
            }
        }

        public IEnumerable<PaymentReportViewModel> GetPaymentResportsByDate(DateTime fromDate, DateTime toDate)
        {
            var paymentReports = new List<PaymentReportViewModel>();

            var selectedReports = _paymentReportRepository.GetAll(predicate: x => x.ReportDate >= fromDate && x.ReportDate <= toDate,
                orderBy: x => x.OrderBy(x => x.ReportDate),
                include: x => x.Include(x => x.Debts));

            var usersIds = selectedReports.Select(x => x.PayerId).Distinct().ToList();
            var users = _userRepository.GetUsersByIds(usersIds);

            if (selectedReports.Any())
            {
                foreach (var report in selectedReports)
                {
                    var razonSocial = users.FirstOrDefault(u => u.IdApplicationUser == report.PayerId).RazonSocial;

                    var paymentReportViewModel = new PaymentReportViewModel
                    {
                        PayerId = report.PayerId,
                        ReportDate = report.ReportDate,
                        Cuit = report.Cuit,
                        RazonSocial = razonSocial is null ? "" : razonSocial,
                        Currency = (int)report.Currency,
                        Amount = report.Amount,
                        DebtIds = report.Debts is not null ? report.Debts.Select(d => d.Id).ToList() : new List<int>(),
                        Type = report.Instrument,
                        Product = report.Product,
                        Status = report.Status,
                        ReportDateVto = report.ReportDateVto.GetValueOrDefault()
                    };

                    paymentReports.Add(paymentReportViewModel);
                }
            }

            return paymentReports;
        }
    }
}
