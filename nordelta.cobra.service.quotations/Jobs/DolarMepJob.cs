using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Services.Contracts;
using nordelta.cobra.service.quotations.Models;
using Quartz;

namespace nordelta.cobra.service.quotations.Jobs
{
    [DisallowConcurrentExecution]
    public class DolarMepJob : IJob
    {
        private readonly ILogger<DolarMepJob> _logger;
        private readonly IQuotationService _quotationService;

        public DolarMepJob(

            IQuotationService quotationService,
            ILogger<DolarMepJob> logger
          ) =>
            (_quotationService, _logger) = (quotationService, logger);

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting DolarMEPJob");

            await _quotationService.GetDolarMepValueAsync();
        }
    }

    public class DolarMEP : Quotation
    {
        public DolarMEP()
        {
            base.RateType = RateTypes.UsdMEP;
            base.FromCurrency = "USD";
            base.ToCurrency = "ARS";
        }
        public string Especie { get; set; }
        public override double Calcular()
        {
            return base.Valor;
        }
    }
}
