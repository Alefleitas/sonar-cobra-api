using BrunoZell.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Services.Contracts;
using System.Collections.Generic;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [AuthToken(new EPermission[] {
        EPermission.Access_Contact
    })]
    public class ContactController : ControllerBase
    {
        private readonly IMailService _mailService;
        public ContactController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost]
        public IActionResult Send([ModelBinder(BinderType = typeof(JsonModelBinder))] ContactViewModel contactForm, IList<IFormFile> attachments)
        {
            //TODO: Tiene que formatear bien el mensaje
            _mailService.sendContactEmail(contactForm, attachments);
            return StatusCode(200);
        }
    }
}