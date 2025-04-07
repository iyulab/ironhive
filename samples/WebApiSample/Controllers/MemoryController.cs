using Microsoft.AspNetCore.Mvc;

namespace WebApiSample.Controllers;

[ApiController]
[Route("/mem")]
public class MemoryController : ControllerBase
{
    public MemoryController()
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        await Task.Delay(10);
        return Ok();
    }
}
