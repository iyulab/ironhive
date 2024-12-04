using Microsoft.AspNetCore.Mvc;
using Raggle.Abstractions;
using System.Reflection;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/system")]
public class SystemController : ControllerBase
{
    [HttpGet("version")]
    public ActionResult<string> GetApplicationVersion()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            return StatusCode(500, "Unable to determine the application version.");
        }

        var version = assembly.GetName().Version;
        return Ok(version);
    }

    [HttpGet("health")]
    public async Task<ActionResult> GetHealthAsync()
    {
        await Task.Delay(1000);
        return Ok("");
    }

    [HttpGet("time")]
    public ActionResult<DateTime> GetCurrentServerTime()
    {
        var serverTimeUtc = DateTime.UtcNow;
        return Ok(serverTimeUtc);
    }
}
