using Microsoft.AspNetCore.Mvc;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    public class ClientProfileController : BaseApiController
    {
        private readonly IClientProfileService _clientProfileService;

        public ClientProfileController(IClientProfileService clientProfileService)
        {
            _clientProfileService = clientProfileService;
        }
        [AuthToken(EPermission.Access_Reports)]
        [HttpGet]
        public IActionResult GetClientProfileControl()
        {
            return ExecuteWithErrorHandling(() =>
            {
                var result = _clientProfileService.GetClientProfileControl();
                return Ok(result);
            });
        }
    }
}