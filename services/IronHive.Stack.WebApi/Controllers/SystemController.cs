using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace IronHive.Stack.WebApi.Controllers;

[ApiController]
[Route("/system")]
public class SystemController : ControllerBase
{
    [HttpGet("healthz")]
    public async Task<ActionResult> GetHealthAsync()
    {
        await Task.CompletedTask;
        return Ok("ok");
    }

    [HttpGet("time")]
    public async Task<ActionResult> GetTimeAsync()
    {
        await Task.CompletedTask;
        return Ok(DateTime.UtcNow);
    }

    [HttpGet("version")]
    public async Task<ActionResult> GetVersionAsync()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            return StatusCode(500, "Unable to determine the application.");
        }

        var version = assembly.GetName().Version;
        await Task.CompletedTask;
        return Ok(version);
    }
}
