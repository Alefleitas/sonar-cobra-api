using Microsoft.AspNetCore.Mvc;
using nordelta.service.middle.itau.Controllers.ActionFilter;
using nordelta.service.middle.itau.Models;
using nordelta.service.middle.itau.Services.DTOs;
using nordelta.service.middle.itau.Services.Interfaces;
using Serilog;
using System.Net;

namespace nordelta.service.middle.itau.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    public class ItauController : ControllerBase
    {
        private readonly IProcessNotificationService _processNotificationService;

        public ItauController(IProcessNotificationService processNotificationService)
        {
            _processNotificationService = processNotificationService;
        }



        [HttpPost]
        [Route("transaction/notification")]
        [SslClientCertActionFilter]
        public async Task<ActionResult> Nordelta([FromBody] TransactionResultDto transactionResult)
        {
            try
            {
                Log.Logger.Information("WebHook Nordelta activado. Detalle: \n {@tr}", transactionResult);
                if (transactionResult == null)
                {
                    Log.Error("Error en el hook ConsultatioWebhook");
                    return BadRequest();
                }

                var result = await _processNotificationService.ProcessNotificationAsync(CompanySocialReason.NordeltaSA, transactionResult);
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(result);
                }
                return BadRequest();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. NordeltaWebhook.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("transaction/notification")]
        [SslClientCertActionFilter]
        public async Task<ActionResult> FideicomisoGolfClub([FromBody] TransactionResultDto transactionResult)
        {
            try
            {
                Log.Information("WebHook FideicomisoGolfClub activado. Detalle: \n {@tr}", transactionResult);
                Log.Debug("WebHook FideicomisoGolfClub activado. Detalle: \n {@tr}", transactionResult);
                if (transactionResult == null)
                {
                    Log.Error("Error en el hook ConsultatioWebhook");
                    return BadRequest();
                }

                var result = await _processNotificationService.ProcessNotificationAsync(CompanySocialReason.FideicomisoGolfClub, transactionResult);
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(result);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. GolfClubWebhook.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("transaction/notification")]
        [SslClientCertActionFilter]
        public async Task<ActionResult> Consultatio([FromBody] TransactionResultDto transactionResult)
        {
            try
            {
                Log.Logger.Information("WebHook Consultatio activado. Detalle: \n {@tr}", transactionResult);
                if (transactionResult == null)
                {
                    Log.Error("Error en el hook ConsultatioWebhook");
                    return BadRequest();
                }

                var result = await _processNotificationService.ProcessNotificationAsync(CompanySocialReason.ConsultatioSA, transactionResult);
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(result);
                }
                return BadRequest();

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. ConsultatioWebhook.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("transaction/notification")]
        [SslClientCertActionFilter]
        public async Task<ActionResult> UtePuertoMadero([FromBody] TransactionResultDto transactionResult)
        {
            try
            {
                Log.Logger.Information("WebHook UtePuertoMadero activado. Detalle: \n {@tr}", transactionResult);
                if (transactionResult == null)
                {
                    Log.Error("Error en el hook UtePuertoMaderoWebhook");
                    return BadRequest();
                }
                var result = await _processNotificationService.ProcessNotificationAsync(CompanySocialReason.UtePuertoMadero, transactionResult);
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(result);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. UtePuertoMadero.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        [Route("transaction/notification")]
        [SslClientCertActionFilter]
        public async Task<ActionResult> UtHuergo([FromBody] TransactionResultDto transactionResult)
        {
            try
            {
                Log.Logger.Information("WebHook UteHuergo activado. Detalle: \n {@tr}", transactionResult);
                if (transactionResult == null)
                {
                    Log.Error("Error en el hook ConsultatioWebhook");
                    return BadRequest();
                }

                var result = await _processNotificationService.ProcessNotificationAsync(CompanySocialReason.UteHuergo, transactionResult);
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return Ok(result);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Internal Error. UteHuergo.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

    }
}
