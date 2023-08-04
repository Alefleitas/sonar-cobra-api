using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using Quartz;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.service.quotations.Jobs
{
    public class QuotationCoinApiJob : IJob
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationCoinApiJob> _logger;
        private readonly ServiciosConfiguration _servicios;

        public QuotationCoinApiJob(IQuotationService quotationService, ILogger<QuotationCoinApiJob> logger, IOptions<ServiciosConfiguration> servicios)
        {
            _quotationService = quotationService;
            _logger = logger;
            _servicios = servicios.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogDebug("Starting call to GetQuotationsFromCoinApiAsync...");
                var quotations = await _quotationService.GetQuotationsFromCoinApiAsync();
                _logger.LogDebug(quotations.Any() ? "GetQuotationsFromCoinApiAsync Successfully" : "GetQuotationsFromCoinApiAsync Failed");
                if (quotations.Any())
                {
                    _logger.LogDebug("Starting call to InformQuotationsAsync...");
                    var result = await _quotationService.InformQuotationsAsync(quotations, "AddQuotationsFromCoinApiExternal");
                    _logger.LogDebug(result ? "InformQuotationsAsync Successfully" : "InformQuotationsAsync Failed");

                    if (result)
                        Monitoreo.Monitor.Ok("Se informaron las cotizaciones obtenidas de CoinApi", _servicios.CoinApi);
                    else
                        Monitoreo.Monitor.Critical("Ocurrió un error al intentar informar las cotizaciones de CoinApi", _servicios.CoinApi);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in QuotationCoinApiJob");
                Monitoreo.Monitor.Critical("Ocurrió un error al obtener las cotizaciones de CoinApi", _servicios.CoinApi);
            }
        }
    }
}
