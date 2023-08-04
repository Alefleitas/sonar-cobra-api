using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using Quartz;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.service.quotations.Jobs
{
    public class CacJob: IJob
    {
        private readonly IQuotationService _quotationService;
        private readonly ILogger<CacJob> _logger;
        private readonly ServiciosConfiguration _servicios;

        public CacJob(
            IQuotationService quotationService,
            ILogger<CacJob> logger,
            IOptions<ServiciosConfiguration> servicios)
        {
            _quotationService = quotationService;
            _logger = logger;
            _servicios = servicios.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogDebug("Starting call to GetCacAsync...");
                var quotation = await _quotationService.GetCacAsync();
                _logger.LogDebug(quotation.Valor > 0 ? "GetCacAsync Successfully" : "GetCacAsync Failed");
                if (quotation.Valor > 0)
                {
                    _logger.LogDebug("Starting call to InformQuotationAsync...");
                    var result = await _quotationService.InformQuotationAsync(quotation, "AddQuotationCacExternal");
                    _logger.LogDebug(result ? "InformQuotationAsync Successfully" : "InformQuotationAsync Failed");

                    if (result)
                        Monitoreo.Monitor.Ok("Se informaron las cotizaciones de CAC obtenidas", _servicios.Cac);
                    else
                        Monitoreo.Monitor.Critical("Ocurrió un error al intentar informar la cotización", _servicios.Cac);
                }
                else
                {
                    Monitoreo.Monitor.Warning("La cotización obtenida no es válida", _servicios.Cac);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CacJob");
                Monitoreo.Monitor.Critical("Ocurrió un error al obtener la cotización de CAC", _servicios.Cac);
            }
        }
    }
}
