using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Avalon.Controllers
{
    //[ApiController]
    //public class ErrorController : Controller
    //{
    //    [Route("/error-local-development")]
    //    public IActionResult ErrorLocalDevelopment(
    //    [FromServices] IWebHostEnvironment webHostEnvironment)
    //    {
    //        if (webHostEnvironment.EnvironmentName != "Development")
    //        {
    //            throw new InvalidOperationException(
    //                "This shouldn't be invoked in non-development environments.");
    //        }

    //        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

    //        return Problem(
    //            detail: context.Error.StackTrace,
    //            title: context.Error.Message);
    //    }

    //    [Route("/error")]
    //    public IActionResult Error() => Problem();
    //}
}
