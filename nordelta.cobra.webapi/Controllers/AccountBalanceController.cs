using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.Contracts;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using RestSharp;

namespace nordelta.cobra.webapi.Controllers
{
    
    public class AccountBalanceController : BaseApiController
    {
        private readonly IAccountBalanceService _accountBalanceService;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        public AccountBalanceController(IAccountBalanceService accountBalanceService, IAccountBalanceRepository accountBalanceRepository)
        {
            _accountBalanceService = accountBalanceService;
            _accountBalanceRepository = accountBalanceRepository;
        }

        [AuthToken(EPermission.Access_CRM)]
        [HttpPost]
        public IActionResult UpdateAccountBalance(AccountBalanceDTO accountBalanceDTO)
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            user.Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail;
            user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;
            
            AccountBalance accountBalance = _accountBalanceRepository.GetAccountBalanceById(accountBalanceDTO.Id);

            if (!AuthFilterBUHelper<AccountBalance>.AuthByBU(accountBalance, user, HttpContext)) return Unauthorized();

            //Solo checkea permisos para el account balance de la BD ya que si no, no podría derivar un account balance a otro departamento
            if (!AuthFilterDepartmentHelper<AccountBalance>.AuthByDepartment(accountBalance, user, HttpContext)) return Unauthorized();
            
            return ExecuteWithErrorHandling(() =>
            {
                var result = _accountBalanceService.UpdateAccountBalance(accountBalanceDTO, user);
                return Ok(result);
            });
        }

        [AuthToken(EPermission.Access_CRM)]
        [HttpGet]
        public IActionResult GetAll([FromQuery]int? limit, [FromQuery] int? page, [FromQuery] string search, [FromQuery]  string project, [FromQuery] int? department, [FromQuery] int? balance)
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            return ExecuteWithErrorHandling(() =>
            {
                var pageSize = limit ?? 10;
                var pageNumber = page ?? 1;
                var accountBalancePagination = new AccountBalancePagination();

                accountBalancePagination = _accountBalanceService.GetAllAccountBalances(user, pageSize, pageNumber, search, project, department, balance);

                return Ok(accountBalancePagination);
            });
        }
        [AuthToken(EPermission.Access_CRM)]
        [HttpGet]
        public IActionResult GetAllForReport([FromQuery] string search, [FromQuery] string project, [FromQuery] int? department, [FromQuery] int? balance)
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            return ExecuteWithErrorHandling(() =>
            {
                var result = _accountBalanceService.GetAllAccountBalances(user, search, project, department, balance);

                return Ok(result);
            });
        }
        [AuthToken(EPermission.Access_CRM)]
        [HttpGet]
        public IActionResult GetAllProjects()
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
            return ExecuteWithErrorHandling(() =>
            {
                bool isExternal = user.Roles.Any(r => r.Name.ToLower() == AccountBalance.EDepartment.Externo.ToString().ToLower());
                                   
                var result = _accountBalanceService.GetAllAccountBalanceBU(isExternal);
                
                result = result.Where(x => user.BusinessUnits.Any(b => b.Name.ToLower() == x.ToLower())).ToList();

                return Ok(result);
            });
        }

        [AuthToken(EPermission.Access_Payments)]
        [HttpGet]
        public IActionResult GetUserProducts()
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            return ExecuteWithErrorHandling(() =>
            {
                var result = _accountBalanceService.GetClientProductsByCuits(user.AdditionalCuits);

                return Ok(result);
            });
        }

        [AuthToken(EPermission.Access_CRM)]
        [HttpGet]
        public IActionResult GetPaymentDetails(int accountBalanceId)
        {
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
            AccountBalance accountBalance = _accountBalanceRepository.GetAccountBalanceById(accountBalanceId);

            if (!AuthFilterBUHelper<AccountBalance>.AuthByBU(accountBalance, user, HttpContext)) return Unauthorized();

            return ExecuteWithErrorHandling(() =>
            {
                var result = _accountBalanceService.GetAccountBalanceDetail(accountBalance);
                return Ok(result);
            });
        }

        [AuthToken(EPermission.Access_CRM)]
        [HttpPost]
        public IActionResult GetDeudaMora(List<DeudaMoraRequestDto> products)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = _accountBalanceService.GetAllDeudaMoraByProduct(products);
                return Ok(result);
            });
        }
    }
}