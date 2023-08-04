using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using nordelta.cobra.service.quotations.Utils;
using Monitoreo = Nordelta.Monitoreo;
using Quartz;

namespace nordelta.cobra.service.quotations.Jobs
{
    [DisallowConcurrentExecution]
    public class FinanceQuotationsJob : IJob
    {
        private readonly ILogger<FinanceQuotationsJob> _logger;
        private readonly IQuoteService _quoteService;
        private readonly IHolidayService _holidayService;
        private readonly ServiciosConfiguration _servicios;

        public FinanceQuotationsJob(
          ILogger<FinanceQuotationsJob> logger,
          IQuoteService quoteService,
          IHolidayService holidayService,
          IOptions<ServiciosConfiguration> servicios) =>
          (_logger, _quoteService, _holidayService, _servicios) = (logger, quoteService, holidayService, servicios.Value);

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting QuotationsDashboardJob");

            try
            {
                if (!(await _holidayService.IsAHolidayAsync(LocalDateTime.GetDateTimeNow(), null)))
                {
                    var quotes = await _quoteService.GetQuotesAsync();

                    if (quotes.Any())
                    {
                        _logger.LogDebug("QuickAccessQuotationsJob - GetQuotesAsync Successful");
                        Monitoreo.Monitor.Ok("QuickAccessQuotationsJob - GetQuotesAsync Successful", _servicios.FinanceQuotations);
                    }

                    var result = await _quoteService.InformQuotesAsync(quotes, "FinanceQuotations");

                    if (result)
                    {
                        _logger.LogDebug("QuickAccessQuotationsJob - InformQuotesAsync Successful");
                        Monitoreo.Monitor.Ok("QuickAccessQuotationsJob - InformQuotesAsync Successful", _servicios.FinanceQuotations);
                    }
                    else
                    {
                        _logger.LogDebug("QuickAccessQuotationsJob - InformQuotesAsync Failed");
                        Monitoreo.Monitor.Critical("QuickAccessQuotationsJob - InformQuotesAsync Failed", _servicios.FinanceQuotations);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("QuickAccessQuotationsJob - Ocurred an error exception", ex.Message);
                Monitoreo.Monitor.Critical("QuickAccessQuotationsJob - Ocurrió un error al obtener todas las cotizaciones", _servicios.FinanceQuotations);
            }
        }

    }
}
