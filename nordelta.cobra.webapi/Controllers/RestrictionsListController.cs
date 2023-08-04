using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    //[AllowAnonymous]
    public class RestrictionsListController : ControllerBase
    {
        private readonly IRestrictionsListService _restrictionsListService;
        public RestrictionsListController(IRestrictionsListService restrictionsListService)
        {
            _restrictionsListService = restrictionsListService;
        }

        [HttpPut]
        [AuthToken(EPermission.Lock_AdvancePayments)]
        public IActionResult SetLockAdvancePayments([FromBody] bool action)
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                if (user == null)
                    Log.Error($"Error: No se encontro usuario. Id: {user.Id}.");

                user.Id = string.IsNullOrEmpty(user.SupportUserId) ? user.Id : user.SupportUserId;
                user.Email = string.IsNullOrEmpty(user.SupportUserEmail) ? user.Email : user.SupportUserEmail;

                var res = _restrictionsListService.SetLockAdvancePayments(action, user);

                return Ok(res);
            }
            catch
            {
                return BadRequest();
            }

        }

        [HttpGet]
        [AuthToken(EPermission.Access_AdvancePayments)]
        public IActionResult GetLockAdvancePayments()
        {
            try
            {
                return Ok(_restrictionsListService.GetLockAdvancePayments().LockedByUser);
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [AuthToken(EPermission.Access_Support)]
        public ActionResult<List<Restriction>> GetCompleteRestrictionsList()
        {
            List<Restriction> restrictionsList = new List<Restriction>();
            try
            {
                restrictionsList = _restrictionsListService.GetCompleteRestrictionsList();
            }
            catch
            {
                Serilog.Log.Error("No se pudo tomar la lista de restricciones");
            }
            return restrictionsList;
        }

        [HttpGet]
        [AuthToken(EPermission.Access_Support)]
        public IActionResult GetRestrictionsListByUserId(string userId)
        {
            try
            {
                var restrictionList = _restrictionsListService.GetRestrictionsListByUserId(userId);
                return Ok(restrictionList);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("No se pudo obtener la lista de restricciones: {@message}", ex.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [AuthToken(EPermission.Access_Support)]
        public bool PostRestrictionList(List<Restriction> newRestrictions)
        {
            try
            {
                return _restrictionsListService.PostRestrictionList(newRestrictions);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error("No se pudo obtener la lista de restricciones: {@message}", ex.Message);
                return false;
            }
        }

        [HttpDelete]
        [AuthToken(EPermission.Access_Support)]
        public bool DeleteRestrictionsByUserId(string userId)
        {
            return _restrictionsListService.DeleteRestrictionsByUserId(userId);
        }
    }
}