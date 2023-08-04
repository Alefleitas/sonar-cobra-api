using System;
using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Helpers;
using nordelta.cobra.webapi.Models;
using nordelta.cobra.webapi.Repositories.Contracts;
using nordelta.cobra.webapi.Services.Contracts;
using nordelta.cobra.webapi.Services.DTOs;
using Serilog;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [Produces(MediaTypeNames.Application.Json)]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService loginService;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository userRepository;
        public LoginController(ILoginService loginService, IConfiguration configuration, IUserRepository userRepository)
        {
            this.loginService = loginService;
            this._configuration = configuration;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public ActionResult<string> Login()
        {
            try
            {
                string ssoToken = HttpContext.Request.Headers["SsoToken"].ToString();
                User user = loginService.GetAuthenticatedUser(ssoToken);
                if (user != null)
                {
                    string cobraToken = JwtManager.GenerateToken(user);
                    return new OkObjectResult(cobraToken);
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. Login, Exception detail: {@ex}", ex);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public ActionResult<ChangePasswordResponse> ChangePassword([FromBody]ChangePasswordRequest changePasswordRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                ChangePasswordResponse result = loginService.ChangePassword(changePasswordRequest); ;
                return result == null
                    ? (ActionResult<ChangePasswordResponse>) StatusCode(StatusCodes.Status400BadRequest)
                    : new OkObjectResult(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost]
        public ActionResult<UpdateUserResponse> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }
                UpdateUserResponse result = loginService.UpdateUser(request); ;
                return result == null
                    ? (ActionResult<UpdateUserResponse>)StatusCode(StatusCodes.Status401Unauthorized)
                    : new OkObjectResult(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [AuthToken(new EPermission[] { EPermission.Access_Support })]
        [HttpPost("{clientId}")]
        public ActionResult<string> LoginAsClient(string clientId)
        {
            try
            {
                User supportUser = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                User clientUser = userRepository.GetUserById(clientId);
                loginService.FilterUserPermissions(ref clientUser);
                if (supportUser != null)
                {
                    string cobraToken = JwtManager.GenerateToken(clientUser, supportUser);
                    return new OkObjectResult(cobraToken);
                }
                return Unauthorized();
            }
            catch (Exception e)
            {
                Serilog.Log.Error(@"Error when logging as a client, @{e}", e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [AuthToken(EPermission.Access_Reports)]
        [HttpGet()]
        public IActionResult GetAllLastAccess()
        {
            try
            {
                var lastAccess = loginService.GetAllLastAccess();
                return new OkObjectResult(lastAccess);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(@"Error GetAllLastAccess, @{e}", e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [AuthToken(EPermission.Access_Reports)]
        [HttpGet()]
        public IActionResult GetAllClientDuplicatedEmails()
        {
            try
            {
                var duplicatedEmails = loginService.GetAllClientDuplicatedMails();
                return new OkObjectResult(duplicatedEmails);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(@"Error GetAllClientDuplicatedEmails, @{e}", e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        [AuthToken(EPermission.Access_Reports)]
        [HttpGet()]
        public IActionResult GetAllCreatedUsers()
        {
            try
            {
                var userCreated = loginService.GetAllCreatedUsers();
                return new OkObjectResult(userCreated);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(@"Error GetAllCreatedUsers, @{e}", e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
