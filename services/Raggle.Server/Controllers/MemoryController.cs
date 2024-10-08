using Microsoft.AspNetCore.Mvc;

namespace Raggle.Server.Controllers;

[ApiController]
[Route("/index")]
public class MemoryController : ControllerBase
{
    private readonly ILogger<MemoryController> _logger;

    public MemoryController(ILogger<MemoryController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            TotalMemory = GC.GetTotalMemory(false),
            CollectionCount = GC.CollectionCount(0)
        });
    }
}
