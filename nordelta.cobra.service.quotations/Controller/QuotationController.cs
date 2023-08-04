using Microsoft.AspNetCore.Mvc;
using nordelta.cobra.service.quotations.Services.Contracts;

namespace nordelta.cobra.service.quotations.Controller
{
    public class QuotationController : BaseApiController
    {
        private readonly ILogger<QuotationController> _logger;
        private readonly IQuotationService _quotationService;

        public QuotationController(ILogger<QuotationController> logger,
                IQuotationService quotationService
            )
        {
            _logger = logger;
            _quotationService = quotationService;
        }


        [HttpGet]
        public async Task<IActionResult> GetBonosConfiguration()
        {
            try
            {
                var bonos = await _quotationService.GetEspeciesAsync();
                return Ok(bonos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllBonos > GetEspeciesAsync > Error: ", ex);
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> GetSourceQuote([FromQuery] DateTime date, [FromBody] IEnumerable<string> quotes)
        {
            try
            {
                var result = await _quotationService.GetSourceQuotationsAsync(date, quotes);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSourceQuote > GetSourceQuotationsAsync > Error: ", ex);
            }
            return BadRequest();

        }


        [HttpGet]
        public async Task<IActionResult> GetDolarMep()
        {
            try
            {
                await _quotationService.GetDolarMepValueAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError("QuotationController > ExecuteDolarMepJobAsync > Error ", ex);
                return BadRequest(ex);

            }
        }
    }
}
