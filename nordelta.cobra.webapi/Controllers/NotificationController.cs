using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using Serilog;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [AuthToken(new EPermission[] { EPermission.Access_CRM })]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        public NotificationController(INotificationService  notificationService, INotificationRepository notificationRepository)
        {
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
        }

        [HttpGet]
        public ActionResult<List<NotificationType>> GetNotificationTypes()
        {
            return new OkObjectResult(_notificationRepository.GetNotificationTypes());
        }

        [HttpPost("{notificationTypeId}")]
        public ActionResult CreateTemplate([FromBody] Template template, int notificationTypeId)
        {
            try
            {
                _notificationService.CreateTemplate(template, notificationTypeId);
                return Ok();
            }
            catch (Exception e)
            {
                Log.Error(e, "Template Creation Failed");
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut]
        public ActionResult UpdateTemplate([FromBody] Template template)
        {
            try {
                _notificationService.UpdateTemplate(template);
                return Ok();
            } 
            catch (Exception e)
            {
                Log.Error(e, "Template Update Failed");
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public ActionResult<List<TemplateTokenReference>> GetTemplateTokenReferences()
        {
            return new OkObjectResult(_notificationRepository.GetAllTemplateReferences());
        }
    }
}