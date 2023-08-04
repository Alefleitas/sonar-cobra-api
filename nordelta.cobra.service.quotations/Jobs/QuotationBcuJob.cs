using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using Quartz;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.service.quotations.Jobs
{
    [DisallowConcurrentExecution]
    public class QuotationBcuJob : IJob
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationBcuJob> _logger;
        private readonly ServiciosConfiguration _servicios;

        public QuotationBcuJob(IQuotationService quotationService, ILogger<QuotationBcuJob> logger, IOptions<ServiciosConfiguration> servicios)
        {
            _quotationService = quotationService;
            _logger = logger;
            _servicios = servicios.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogDebug("Starting call to GetQuotationsFromBcuAsync...");
                var quotations = await _quotationService.GetQuotationsFromBcuAsync();
                _logger.LogDebug(quotations.Any() ? "GetQuotationsFromBcuAsync Successfully" : "GetQuotationsFromBcuAsync Failed");
                if (quotations.Any())
                {
                    _logger.LogDebug("Starting call to InformQuotationsAsync...");
                    var result = await _quotationService.InformQuotationsAsync(quotations, "AddQuotationsFromBcuExternal");
                    _logger.LogDebug(result ? "InformQuotationsAsync Successfully" : "InformQuotationsAsync Failed");

                    if (result)
                        Monitoreo.Monitor.Ok("Se informaron las cotizaciones obtenidas de BCU", _servicios.Bcu);
                    else
                        Monitoreo.Monitor.Critical("Ocurrió un error al intentar informar las cotizaciones de BCU", _servicios.Bcu);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuotationBcuJob");
                Monitoreo.Monitor.Critical("Ocurrió un error al obtener las cotizaciones de BCU", _servicios.Bcu);
            }
        }
    }
}
