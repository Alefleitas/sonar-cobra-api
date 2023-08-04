using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    [AuthToken(new EPermission[] { EPermission.Access_Support })]
    public class SupportController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        public SupportController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        
        [HttpGet]
        public ActionResult<List<ClientViewModel>> GetAllClients()
        {
            var ssoUsers = userRepository.GetAllUsers().Where(x => x.Roles.Any(y => y.Role == "Cliente")).ToList();
            return ssoUsers.Select(x => new ClientViewModel
            {
                Id = x.IdApplicationUser,
                Cuit = x.Cuit,
                RazonSocial = x.RazonSocial
            }).ToList();
        }
    }
}
