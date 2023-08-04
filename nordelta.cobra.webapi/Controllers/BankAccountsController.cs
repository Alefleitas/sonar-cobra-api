using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using Serilog;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    public class BankAccountsController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;
        private readonly IMapper _mapper;

        public BankAccountsController(IBankAccountService bankAccountService, IMapper mapper)
        {
            _bankAccountService = bankAccountService;
            _mapper = mapper;
        }

        [AuthToken(new EPermission[] {
            EPermission.Access_Payments
        })]
        [HttpGet]
        public ActionResult<List<BankAccount>> GetClientBankAccounts()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = user.Cuit.ToString();


                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                List<BankAccount> result;
                if (user.AdditionalCuits?.Count >= 2) //Has multiple cuits
                {
                    result = _bankAccountService.GetBankAccountsForClient(user.AdditionalCuits);
                }
                else
                {
                    result = user.IsForeignCuit ? _bankAccountService.GetBankAccountsForClient(cuit, user.AccountNumber) : _bankAccountService.GetBankAccountsForClient(cuit);
                }

                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] {
            EPermission.Access_Extern_Payments
        })]
        [HttpGet]
        public ActionResult<List<ExternalBankAccountViewModel>> ExtGetClientBankAccounts(string clientCuit)
        {
            try
            {
                List<BankAccount> result = _bankAccountService.GetBankAccountsForClient(clientCuit);
                if (!result.Any())
                {
                    return StatusCode(StatusCodes.Status204NoContent);
                }

                return new OkObjectResult(_mapper.Map<List<ExternalBankAccountViewModel>>(result));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] {
            EPermission.Access_Payments
        })]
        [HttpPost]
        public ActionResult<ValidateBankAccountResponse> ValidateBankAccount(BankAccountViewModel bankAccountViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ValidateBankAccountResponse bankAccount = _bankAccountService.ValidateBankAccount(bankAccountViewModel.Cbu, bankAccountViewModel.Cuit);
                return new OkObjectResult(bankAccount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Extern_Payments })]
        [HttpPost]
        public ActionResult<ExternalValidateBankAccountResponse> ValidateExternalBankAccount(ExternalBankAccountViewModel externalBankAccountViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                ValidateBankAccountResponse bankAccount =
                    _bankAccountService.ValidateBankAccount(externalBankAccountViewModel.Cbu, externalBankAccountViewModel.Cuit);

                return new OkObjectResult(_mapper.Map<ExternalValidateBankAccountResponse>(bankAccount));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] {
            EPermission.Access_Payments
        })]
        [HttpPost]
        public ActionResult<BankAccountViewModel> PostClientBankAccount(BankAccountViewModel bankAccountViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

                user.Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail;
                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;

                string cuit = user.Cuit.ToString();

                var existingBankAccount = user.IsForeignCuit ? _bankAccountService.GetBankAccountFromCbu(cuit, bankAccountViewModel.Cbu,
                    bankAccountViewModel.Currency, user.AccountNumber) : _bankAccountService.GetBankAccountFromCbu(cuit, bankAccountViewModel.Cbu,
                    bankAccountViewModel.Currency);

                if (existingBankAccount != null)
                {
                    return StatusCode(StatusCodes.Status302Found);
                }

                var bankAccount = _bankAccountService.AddBankAccount(cuit, bankAccountViewModel.Cbu, bankAccountViewModel.Cuit, bankAccountViewModel.Currency, user);

                var result = new BankAccountViewModel()
                {
                    Id = bankAccount.Id,
                    Cuit = bankAccount.Cuit,
                    Cbu = bankAccount.Cbu,
                    Currency = bankAccount.Currency
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in PostClientBankAccount.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] {
            EPermission.Access_Payments
        })]
        [HttpDelete]
        public ActionResult DeleteClientBankAccount(int id)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                user.Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail;
                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;
                string cuit = user.Cuit.ToString();

                List<BankAccount> bankAccounts;
                if (user.AdditionalCuits?.Count >= 2) //Has multiple cuits
                    bankAccounts = _bankAccountService.GetBankAccountsForClient(user.AdditionalCuits);
                else
                    bankAccounts = user.IsForeignCuit ? _bankAccountService.GetBankAccountsForClient(cuit, user.AccountNumber) : _bankAccountService.GetBankAccountsForClient(cuit);

                if (bankAccounts.Any(x => x.Id == id))
                {
                    var result = _bankAccountService.DeleteBankAccount(id, user);

                    return result ? StatusCode(StatusCodes.Status200OK) : StatusCode(StatusCodes.Status400BadRequest);
                }
                else
                {
                    Serilog.Log.Warning($"La BankAccount con Id {id} no pertenece al cliente");
                    return StatusCode(StatusCodes.Status404NotFound);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}