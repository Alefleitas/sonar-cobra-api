using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Produces("application/json")]
    public class BaseApiController : ControllerBase
    {
        protected IActionResult ExecuteWithErrorHandling(Func<IActionResult> action)
        {
            try
            {
                return action();
            }
            catch (ApplicationException ex)
            {
                Log.Error("Application Exception. Exception detail: {@ex}", ex);
                Console.WriteLine(ex);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Internal Error. Exception detail: {@ex}", ex);
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}