using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using Quartz;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.service.quotations.Jobs
{
    [DisallowConcurrentExecution]
    public class QuotationBnaJob : IJob
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<QuotationBnaJob> _logger;
        private readonly ServiciosConfiguration _servicios;

        public QuotationBnaJob(IQuotationService quotationService, ILogger<QuotationBnaJob> logger, IOptions<ServiciosConfiguration> servicios)
        {
            _quotationService = quotationService;
            _logger = logger;
            _servicios = servicios.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogDebug("Starting call to GetQuotationsFromBnaAsync...");
                var quotations = await _quotationService.GetQuotationsFromBnaAsync();
                _logger.LogDebug(quotations.Any() ? "GetQuotationsFromBnaAsync Successfully" : "GetQuotationsFromBnaAsync Failed");
                if (quotations.Any())
                {
                    _logger.LogDebug("Starting call to InformQuotationsAsync...");
                    var result = await _quotationService.InformQuotationsAsync(quotations, "AddQuotationsFromBnaExternal");
                    _logger.LogDebug(result ? "InformQuotationsAsync Successfully" : "InformQuotationsAsync Failed");

                    if (result)
                        Monitoreo.Monitor.Ok("Se informaron las cotizaciones obtenidas de BNA", _servicios.Bna);
                    else
                        Monitoreo.Monitor.Critical("Ocurrió un error al intentar informar las cotizaciones de BNA", _servicios.Bna);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuotationBnaJob");
                Monitoreo.Monitor.Critical("Ocurrió un error al obtener las cotizaciones de BNA", _servicios.Bna);
            }
        }
    }
}

