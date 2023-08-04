using Microsoft.AspNetCore.Mvc;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Exceptions;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using System.Collections.Generic;
using System.Linq;
using nordelta.cobra.webapi.Services.Contracts;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    //[AuthToken(new EPermission[] {
    //    EPermission.Access_Configuration
    //    })]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAccountBalanceService _accountBalanceService;
        public RolesController(IRoleRepository roleRepository, IAccountBalanceService accountBalanceService)
        {
            _roleRepository = roleRepository;
            _accountBalanceService = accountBalanceService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> All()
        {
            return _roleRepository.get().Select(x => x.Name).ToList();
        }
    }
}
