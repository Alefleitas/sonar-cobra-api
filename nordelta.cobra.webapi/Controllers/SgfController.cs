using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;
using nordelta.cobra.webapi.Services.Contracts;
using Serilog;
using nordelta.cobra.webapi.Services.DTOs;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Controllers
{
    public class SgfController : BaseApiController
    {
        private readonly IPaymentMethodService _paymentMethodService;

        public SgfController(IPaymentMethodService paymentMethodService)
        {
            _paymentMethodService = paymentMethodService;
        }

        [HttpPost]
        [AuthToken(new EPermission[] { EPermission.Access_Payments })]
        public ActionResult PaymentInform([FromBody] PaymentInformedDto paymentInformDto)
        {
            try
            {
                if (_paymentMethodService.UpdatePaymentInformStatus(paymentInformDto))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Ocurrió un error al actualizar estado del pago");
                }
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. PaymentInformWebhook, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status422UnprocessableEntity);
            }
        }
    }
}