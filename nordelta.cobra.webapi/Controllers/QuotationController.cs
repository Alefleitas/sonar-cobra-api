using CoinAPI.REST.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Utils;
using nordelta.cobra.webapi.Websocket;
using NPOI.POIFS.Crypt.Dsig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    public class QuotationController : ControllerBase
    {
        private readonly IExchangeRateFilesService _exchangeRateService;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<FinanceQuotationsHub> _hubContext;
        private readonly IFinanceQuotationsService _financeQuotationsService;

        public QuotationController(
            IExchangeRateFilesService exchangeRateService,
            IConfiguration configuration,
            IHubContext<FinanceQuotationsHub> hubContext,
            IFinanceQuotationsService financeQuotationsService
        )
        {
            _exchangeRateService = exchangeRateService;
            _configuration = configuration;
            _hubContext = hubContext;
            _financeQuotationsService = financeQuotationsService;
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpGet]
        public ActionResult<List<QuotationViewModel>> GetQuotations()
        {
            try
            {
                List<QuotationViewModel> quotations = _exchangeRateService.GetQuotationTypes();
                return new OkObjectResult(quotations);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error retreiving quotations: {@error}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpGet]
        public ActionResult<List<string>> GetSourceTypes()
        {
            try
            {
                return new OkObjectResult(_exchangeRateService.GetSourceTypes());
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error retreiving quotations: {@error}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Rates)]
        [HttpGet]
        public ActionResult<Quotation> GetLastQuotation([FromQuery] string quotationType)
        {
            try
            {
                dynamic quotation = _exchangeRateService.GetLastQuotation(quotationType);
                return new OkObjectResult(quotation);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Unknown provided type {type}: {@error}", quotationType, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Rates)]
        [HttpGet]
        public ActionResult<double> GetCurrentQuotation([FromQuery] string quotationType)
        {
            try
            {
                dynamic quotation = _exchangeRateService.GetCurrentQuotation(quotationType);
                return new OkObjectResult(quotation);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Unknown provided type {type}: {@error}", quotationType, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public dynamic AddQuotation([FromQuery] string quotationType, dynamic quotation)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                user.Id = !string.IsNullOrEmpty(user.SupportUserId) ? user.SupportUserId : user.Id;

                return _exchangeRateService.AddQuotation(quotationType, quotation, user, EQuotationSource.MANUAL);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error adding quotation of type {type}: {@error}", quotationType, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpGet]
        public async Task<IActionResult> GetBonosConfiguration()
        {
            try
            {
                var bonos = await _exchangeRateService.GetEspeciesAsync();
                return Ok(bonos);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "QuotationController> GetAllBonos > GetEspeciesAsync > Error: ", ex);
            }
            return BadRequest();
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public async Task<IActionResult> GetSourceQuoteByDate([FromQuery] DateTime date, [FromBody] IEnumerable<string> quotes)
        {
            try
            {
                var quotations = await _exchangeRateService.GetSourceQuotationsAsync(date, quotes);
                var result =  _exchangeRateService.CheckQuotationsExists(quotations, date);
                var response = new
                {
                    quotationsOrigen = quotations,
                    generatedQuotations = result
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "QuotationController > PublishQuoteByDate > SaveQuotesByDateAsync > Error: ", ex);
            }
            return BadRequest();

        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public async Task<ActionResult> PublishQuotationByBono([FromBody] IEnumerable<Bono> bonos)
        {
            try
            {
                 _exchangeRateService.PublishQuotationByBono(bonos);
                var wasExecuted = _exchangeRateService.CheckDolarMepJobWasExecuted();
                if (wasExecuted)
                {
                    return Ok(_exchangeRateService.ExecuteGetDolarMepAsync());
                }
                return Ok(StatusCodes.Status200OK);

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error publishing quotation by bonos: {@error}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpGet]
        public ActionResult<List<Bono>> GetBonosConfig()
        {
            try
            {
                return Ok(_exchangeRateService.GetLastBonosConfig());
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error getting bonos: {@error}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult InformToSystems([FromBody] dynamic data)
        {
            try
            {
                var result = _exchangeRateService.AddQuotationsAndInform(data);
                if (result.Count > 0)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error al informar cotizaciones: {@error}", ex);
            }
            return Ok();
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult AddDateQuotesExternal([FromBody] List<QuotationExternal> quotations)
        {
            try
            {
                string Msg;
                if (quotations.Count == 0)
                {
                    Msg = "No quotes received from BCU";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }
                else if (quotations.FirstOrDefault(x => x.Valor == 0) != null)
                {
                    Msg = "One or more quotes received from BCU has value 0";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }

                var dateQuotes = _exchangeRateService.GenerateQuotations(quotations);
                var result = _exchangeRateService.AddQuotations(dateQuotes, new User { Id = "ServiceWorker" });

                if (result.Count > 0)
                {
                    Serilog.Log.Information($"new Quotations from BCU: {JToken.Parse(JsonConvert.SerializeObject(result))}");
                    return Ok(result);
                }
                else
                    Serilog.Log.Warning("Warning: Could not add any quotation from BCU");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception: Error when adding quotes from BCU");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult<int> AddMepQuotationExternal([FromQuery] string quotationType, DolarMEP quotation)
        {
            try
            {
                var result = _exchangeRateService.AddQuotation(quotationType, quotation, new Models.User { Id = "ServiceWorker" }, EQuotationSource.INVERTIRONLINE);

                return Ok(result?.Id ?? 0);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error adding quotation of type {type}: {@error}", quotationType, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult AddQuotationsFromBcuExternal(List<QuotationExternal> quotations)
        {
            try
            {
                string Msg;
                if (quotations.Count == 0)
                {
                    Msg = "No quotes received from BCU";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }
                else if (quotations.FirstOrDefault(x => x.Valor == 0) != null)
                {
                    Msg = "One or more quotes received from BCU has value 0";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }

                var newQuotations = _exchangeRateService.GenerateQuotations(quotations, EQuotationSource.BCU);
                var result = _exchangeRateService.AddQuotations(newQuotations, new User { Id = "ServiceWorker" });

                if (result.Count > 0)
                {
                    Serilog.Log.Information($"new Quotations from BCU: {JToken.Parse(JsonConvert.SerializeObject(result))}");
                    return Ok(result);
                }
                else
                    Serilog.Log.Warning("Warning: Could not add any quotation from BCU");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception: Error when adding quotes from BCU");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult AddQuotationsFromBnaExternal(List<QuotationExternal> quotations)
        {
            try
            {
                string Msg;
                if (quotations.Count == 0)
                {
                    Msg = "No quotes received from BNA";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }
                else if (quotations.FirstOrDefault(x => x.Valor == 0) != null)
                {
                    Msg = "One or more quotes received from BNA has value 0";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }

                var newQuotations = _exchangeRateService.GenerateQuotations(quotations, EQuotationSource.BNA);
                var result = _exchangeRateService.AddQuotations(newQuotations, new User { Id = "ServiceWorker" });

                if (result.Count > 0)
                {
                    Serilog.Log.Information($"new Quotations from BNA: {JToken.Parse(JsonConvert.SerializeObject(result))}");
                    return Ok(result);
                }
                else
                    Serilog.Log.Warning("Warning: Could not add any quotation from BNA");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception: Error when adding quotes from BNA");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult AddQuotationFromBcraExternal(QuotationExternal quotation)
        {
            try
            {
                if (quotation.Valor == 0)
                    return BadRequest("The quotation provided by BCRA has a value of 0");

                var newQuotation = _exchangeRateService.GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency);
                var result = _exchangeRateService.AddQuotation(null, newQuotation, new User { Id = "ServiceWorker" }, EQuotationSource.BCRA);

                if (result != null)
                {
                    Serilog.Log.Information($"new Quotation from BCRA: {JToken.Parse(JsonConvert.SerializeObject(result))}");
                    return Ok(result);
                }
                else
                    Serilog.Log.Warning("Warning: Could not add any quotation from BCRA");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"Exception: Error adding quotation of type {quotation.RateType} from BCRA");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult<int> AddQuotationCacExternal(QuotationExternal quotation)
        {
            try
            {
                if (quotation.Valor == 0)
                    return BadRequest("The quotation provided by CAC has a value of 0.");

                var webData = quotation.Description[(quotation.Description.IndexOf('-') + 2)..];

                if (!_exchangeRateService.CheckCacExists(webData))
                {
                    var newQuotation = _exchangeRateService.GenerateQuotation(quotation.Source, quotation.RateType, quotation.Valor, quotation.FromCurrency);
                    newQuotation.Description = quotation.Description;
                    var result = _exchangeRateService.AddQuotation(null, newQuotation, new User { Id = "ServiceWorker" }, EQuotationSource.CAMARCO);

                    if (result != null)
                    {
                        Serilog.Log.Information($"new Quotation CAC: {JToken.Parse(JsonConvert.SerializeObject(result))}");
                        return Ok(result);
                    }
                    else
                        Serilog.Log.Warning("Warning: Could not add any quotation CAC");
                }
                else
                {
                    return Ok("Exists a current quotation of CAC");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error adding quotation CAC of type {type} : {@error}", quotation.RateType, ex);
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public ActionResult<List<int>> AddQuotationsFromCoinApiExternal(List<QuotationExternal> quotations)
        {
            try
            {
                string Msg;
                if (quotations.Count == 0)
                {
                    Msg = "No quotes received from CoinApi";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }
                else if (quotations.FirstOrDefault(x => x.Valor == 0) != null)
                {
                    Msg = "One or more quotes received from CoinApi has value 0";
                    Serilog.Log.Warning(Msg);
                    return BadRequest(Msg);
                }

                var newQuotations = _exchangeRateService.GenerateQuotations(quotations, EQuotationSource.COINAPI);
                var result = _exchangeRateService.AddCryptocurrencyQuotations(newQuotations, new User { Id = "ServiceWorker" });

                if (result.Count > 0)
                {
                    Serilog.Log.Information($"new Quotations from CoinApi: {JToken.Parse(JsonConvert.SerializeObject(result))}");
                    return Ok(result);
                }
                else
                    Serilog.Log.Warning("Warning: Could not add any quotation from CoinApi");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception: Error when adding quotes from CoinApi");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [AuthToken(EPermission.Access_Quotations)]
        [HttpPost]
        public async Task<ActionResult<List<FinanceQuotation>>> FinanceQuotations(List<FinanceQuotation> quotes)
        {
            try
            {
                // Actualiza las cotizaciones
                _financeQuotationsService.UpdateQuotations(quotes);
                // Envía data actualizada a todos los clientes conectados al websocket
                await _hubContext.Clients.All.SendAsync(WebsocketCommands.UpdateQuotations, _financeQuotationsService.GetQuotations());
                // Se guardan en la db las cotizaciones
                _financeQuotationsService.SaveQuotations();

                return Ok(quotes);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Exception: Error when adding quotations from FinanceQuotations");
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("{encryptedHash}")]
        public ActionResult CancellQuotation(string encryptedHash)
        {
            try
            {
                var encryptedID = encryptedHash.Split('.')[0];
                var iv = encryptedHash.Split('.')[1];
                var quotationId = Int32.Parse(AesManager.DecryptFromUrl(encryptedID, _configuration.GetSection("JwtKey").Value, iv));
                var result = _exchangeRateService.CancelQuotation(quotationId);
                if (result)
                    return new OkObjectResult("La cotización fue cancelada correctamente");
                else
                    return new OkObjectResult("No se pudo cancelar la cotización: La cotización no existe o el tiempo para su cancelación expiró");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Quotation Cancellation Hash not valid: {@error}", ex);
                return new OkObjectResult("No se pudo cancelar la cotización: HASH INVALIDO");
            }
        }

        [AuthToken(EPermission.Access_Rates)]
        [HttpGet]
        public ActionResult<double> GetLastDetalleDeudaUSD()
        {
            try
            {
                return new OkObjectResult(_exchangeRateService.GetLastUsdFromDetalleDeuda());
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error getting Last Detalle Deuda USD: {@error}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(EPermission.Access_Rates)]
        [HttpGet("{type}")]
        public ActionResult<List<object>> GetAllQuotations(string type)
        {
            try
            {
                return new OkObjectResult(_exchangeRateService.GetAllQuotations(type));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(@"Exception: Error getting all quotations of {type}: {@error}", type, ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
