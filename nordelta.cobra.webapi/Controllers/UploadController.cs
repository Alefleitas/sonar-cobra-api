using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using nordelta.cobra.webapi.Controllers.ViewModels;
using nordelta.cobra.webapi.Models;
using Serilog;

namespace nordelta.cobra.webapi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [ApiController]
    [Produces("application/json")]
    [AuthToken(new EPermission[]
    {
        EPermission.Access_Payments
    })]
    public class UploadController : ControllerBase
    {
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult<bool> Upload()
        {
            try
            {
                User user = ((User)JsonConvert.DeserializeObject(HttpContext.Request.Headers["user"], typeof(User)));
                string cuit = user.Cuit.ToString();
                string targetFileName = Request.Form["fileName"].FirstOrDefault();
                if (!string.IsNullOrEmpty(targetFileName))
                {
                    targetFileName = targetFileName.Replace("/", "-");
                }
                IFormFile file = Request.Form.Files[0];
                string folderName = Path.Combine("ClientFiles", $"client_{cuit}");
                string pathToSave = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
                Directory.CreateDirectory(pathToSave);

                string dbPath = string.Empty;
                if (file.Length > 0)
                {
                    string fileName = string.IsNullOrEmpty(targetFileName) ? ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"') : targetFileName;

                    string fullPath = Path.Combine(pathToSave, fileName);
                    dbPath = Path.Combine(folderName, fileName);
                    using (FileStream stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }


                return new OkObjectResult(new
                {
                    path = dbPath
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "FileUpload Failed");
                Console.WriteLine(e);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}