using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using Serilog;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    [AuthToken(EPermission.Access_Payments)]
    public class AutomaticPaymentsController : ControllerBase
    {
        IAutomaticDebinRepository _automaticDebinRepository;
        IBankAccountRepository _bankAccountRepository;

        public AutomaticPaymentsController(IAutomaticDebinRepository automaticDebinRepository, IBankAccountRepository bankAccountRepository)
        {
            _automaticDebinRepository = automaticDebinRepository;
            _bankAccountRepository = bankAccountRepository;
        }

        [HttpGet]
        public ActionResult<List<AutomaticPayment>> All()
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            List<AutomaticPayment> result;
            if (AuthTokenAttribute.HasPermissions(user, new EPermission[] { EPermission.Access_EverybodysPayments }))
            {
                result = _automaticDebinRepository.All();
            }
            else
            {
                result = _automaticDebinRepository.All(user);

            }

            return Ok(result);
        }

        [HttpDelete]
        public ActionResult Delete(int automaticPaymentId)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                var auxAutomaticPayment = _automaticDebinRepository.Get(automaticPaymentId);

                if (auxAutomaticPayment.Payer.Id == user.Id)
                    _automaticDebinRepository.Delete(automaticPaymentId);
                else {
                    Log.Logger.Information("El Id de usuario {id} no coincide con el del payer {payerId}", user.Id, auxAutomaticPayment.Payer.Id);
                    return Unauthorized();
                }

                return Ok();
            }
            catch (Exception e)
            {
                Log.Logger.Error(@"Error al borrar el Debito Automático con Id {id}: {@error}", automaticPaymentId, e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public ActionResult New([FromBody] AutomaticPaymentDTO automaticPaymentDTO)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                var bankAccount = _bankAccountRepository.Get(automaticPaymentDTO.BankAccountId);


                //Valida que el usuario sea el del token, y que tenga permiso para utilizar la BankAccount
                if (automaticPaymentDTO.PayerId == user.Id && user.AdditionalCuits.Any(x => x == bankAccount.ClientCuit))
                {
                    if (bankAccount.Currency != automaticPaymentDTO.Currency)
                        return BadRequest("BankAccount's currency must match AutomaticPaymentDTO's currency");

                    var automaticPayment = new AutomaticPayment
                    {
                        BankAccount = bankAccount,
                        Currency = automaticPaymentDTO.Currency,
                        Payer = user,
                        Product = automaticPaymentDTO.Product.Trim(),
                        Id = 0
                    };
                    _automaticDebinRepository.Add(automaticPayment);
                    return Ok();
                }
                else
                    return Unauthorized();


            }
            catch (Exception e)
            {
                Log.Logger.Error(@"Error al crear el Debito Automático {@da}: {@error}",automaticPaymentDTO, e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}