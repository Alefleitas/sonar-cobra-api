using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using Quartz;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.service.quotations.Jobs
{
    [DisallowConcurrentExecution]
    public class QuotationBcraJob : IJob
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationBcraJob> _logger;
        private readonly ServiciosConfiguration _servicios;

        public QuotationBcraJob(IQuotationService quotationService, ILogger<QuotationBcraJob> logger, IOptions<ServiciosConfiguration> servicios)
        {
            _quotationService = quotationService;
            _logger = logger;
            _servicios = servicios.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogDebug("Starting call to GetUsdMayoristaFromBcraAsync...");
                var quotation = await _quotationService.GetUsdMayoristaFromBcraAsync();
                _logger.LogDebug(quotation.Valor > 0 ? "GetUsdMayoristaFromBcraAsync Successfully" : "GetUsdMayoristaFromBcraAsync Failed");
                if (quotation.Valor > 0)
                {
                    _logger.LogDebug("Starting call to InformQuotationAsync...");
                    var result = await _quotationService.InformQuotationAsync(quotation, "AddQuotationFromBcraExternal");
                    _logger.LogDebug(result ? "InformQuotationAsync Successfully" : "InformQuotationAsync Failed");

                    if (result)
                        Monitoreo.Monitor.Ok("Se informaron las cotizaciones obtenidas de BCRA", _servicios.Bcra);
                    else
                        Monitoreo.Monitor.Critical("Ocurrió un error al intentar informar las cotizaciones de BCRA", _servicios.Bcra);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in QuotationBcraJob");
                Monitoreo.Monitor.Critical("Ocurrió un error al obtener las cotizaciones de BCRA", _servicios.Bcra);
            }
        }
    }
}
