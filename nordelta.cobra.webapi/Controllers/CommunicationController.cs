using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.Helpers;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    [AuthToken(EPermission.Access_CRM)]
    public class CommunicationController : BaseApiController
    {
        private readonly ICommunicationService _communicationService;
        private readonly ICommunicationRepository _communicationRepository;
        private readonly IAccountBalanceRepository _accountBalanceRepository;
        private readonly IContactDetailRepository _contactDetailRepository;

        public CommunicationController(ICommunicationService communicationService, ICommunicationRepository communicationRepository, IAccountBalanceRepository accountBalanceRepository, IContactDetailRepository contactDetailRepository)
        {
            this._communicationService = communicationService;
            _communicationRepository = communicationRepository;
            _accountBalanceRepository = accountBalanceRepository;
            this._contactDetailRepository = contactDetailRepository;
        }

        [HttpPost]
        public IActionResult CreateOrUpdate(CommunicationViewModel model)
        {
            AccountBalance accountBalance = _accountBalanceRepository.GetAccountBalanceById((int)model.communication.AccountBalanceId);
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;

            model.communication.Client.Id = accountBalance.ClientId;
            // contacto de Oracle
            if (model.correoElectronico != null)
            {
                // verifico si ya existe
                if (_contactDetailRepository.GetByCorreoElectronico(model.correoElectronico) != null)
                {
                    model.communication.ContactDetailId = _contactDetailRepository.GetByCorreoElectronico(model.correoElectronico).Id;
                }
                else
                {
                    ContactDetail contactDetail = new ContactDetail
                    {
                        Description = "oracle",
                        UserId = model.communication.Client.Id,
                        ComChannel = EComChannelType.CorreoElectronico,
                        Value = model.correoElectronico
                    };
                    model.communication.ContactDetailId = _contactDetailRepository.InsertOrUpdate(contactDetail, user).Id;
                }
            }

            // contacto de la DB
            if (model.communication.ContactDetailId != null)
                model.communication.ContactDetail = _contactDetailRepository.GetById(model.communication.ContactDetailId.Value);

            if (!AuthFilterBUHelper<AccountBalance>.AuthByBU(accountBalance, user, HttpContext)) return Unauthorized();
            if (!AuthFilterDepartmentHelper<AccountBalance>.AuthByDepartment(accountBalance, user, HttpContext)) return Unauthorized();

            return ExecuteWithErrorHandling(() =>
            {
                var result = _communicationService.InsertOrUpdate(model.communication, user);
                if (model.communication.AccountBalanceId.HasValue)
                {
                    _accountBalanceRepository.UpdateStatus(model.communication.AccountBalanceId.Value,
                        AccountBalance.EContactStatus.Contactado);
                }
                return Ok(result);
            });
        }

        [HttpGet]
        public IActionResult GetCommunications(int accountBalanceId)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var communications = _communicationService.GetCommunicationsForAccountBalance(accountBalanceId);

                return Ok(communications);
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Communication communication = _communicationRepository.GetById(id);
            User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));

            user.Id = !string.IsNullOrEmpty(user.SupportUserId) ? user.SupportUserId : user.Id;
            user.Email = !string.IsNullOrEmpty(user.SupportUserEmail) ? user.SupportUserEmail : user.Email;

            if (!AuthFilterBUHelper<Communication>.AuthByBU(communication, user, HttpContext)) return Unauthorized();
            if (!AuthFilterDepartmentHelper<Communication>.AuthByDepartment(communication, user, HttpContext)) return Unauthorized();

            return ExecuteWithErrorHandling(() =>
            {
                var result = _communicationService.Delete(id, user);
                return Ok(result);
            });
        }
        
        [HttpPut("{id}")]
        public IActionResult ToggleTemplate(int id)
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = _communicationService.TemplateToggle(id);
                return Ok(result);
            });
        }

        [HttpPost]
        public IActionResult CreateCommunicationFromService(List<CommunicationFromServiceViewModel> emails)
        {
            return ExecuteWithErrorHandling(() =>
            {
                _communicationService.HandleCommunicationsFromService(emails);

                return Ok();
            });
        }
    }
}