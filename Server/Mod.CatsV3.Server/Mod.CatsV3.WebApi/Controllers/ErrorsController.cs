using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.CatsV3.Application.Dtos;
using Mod.CatsV3.Application.Services;
using Mod.CATSV3.Logs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mod.CatsV3.WebApi.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    public class ErrorsController : ControllerBase
    {
        IServiceScopeFactory ServiceProvider;
        EmailLogs emailLogs;
        public ErrorsController(ILogger<CatsAppService> logger,  IServiceScopeFactory ServiceProvider)
        {
            this.ServiceProvider = ServiceProvider;
            this.emailLogs = new EmailLogs(this.ServiceProvider);
        }
        [Route("/error")]
        public IActionResult ErrorLocalDevelopment([FromServices] IWebHostEnvironment webHostEnvironment)
        {
            if (webHostEnvironment.EnvironmentName != "Development")
            {
                //throw new InvalidOperationException(
                //    "This shouldn't be invoked in non-development environments.");
            }

            var exception = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var result = JsonConvert.SerializeObject(new { error = Problem(
                detail: exception.Error.StackTrace,
                title: exception.Error.Message)
            });


            //return BadRequest(result);
            //emailLogs.logTransactions(null, new CorrespondenceDto(), new Exception(exception.Error.Message));

            return Problem(
                detail: exception.Error.StackTrace,
                title: exception.Error.Message, 
                statusCode: exception.Error.Message == "Session Expired. Please Refresh your browser" ? 440 : 5000);
        }
    }
}
