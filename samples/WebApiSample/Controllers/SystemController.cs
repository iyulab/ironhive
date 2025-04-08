using Microsoft.AspNetCore.Mvc;
using WebApiSample.Services;
using WebApiSample.Settings;
using static WebApiSample.Services.AppService;

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

    [HttpGet("info")]
    public async Task<IActionResult> GetSystemInfoAsync()
    {
        // 서비스가 준비되지 않은 경우 잠깐 대기
        if (_service.CurrentState.Status == AppStatus.Loading)
        {
            await Task.Delay(1_000);
        }

        return Ok(new
        {
            State = _service.CurrentState,
            Settings = _service.CurrentSettings,
        });
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateSettingsAsync(AppServicesSettings settings)
    {
        try
        {
            await settings.SaveAsync(AppConstants.SettingsFilePath);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
