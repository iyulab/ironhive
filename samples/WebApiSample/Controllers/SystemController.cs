using Microsoft.AspNetCore.Mvc;
using WebApiSample.Entities;
using WebApiSample.Services;

namespace WebApiSample.Controllers;

[ApiController]
[Route("/sys")]
public class SystemController : ControllerBase
{
    private readonly AppService _service;

    public SystemController(AppService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatusAsync()
    {
        await Task.Delay(10);
        return Ok(_service.Status);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettingsAsync()
    {
        await Task.Delay(10);
        return Ok();
    }

    [HttpPost("settings")]
    public async Task<IActionResult> UpdateSettingsAsync(ServicesSettings settings)
    {
        await Task.Delay(10);
        return Ok();
    }
}
