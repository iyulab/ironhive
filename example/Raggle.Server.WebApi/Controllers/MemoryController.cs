using Microsoft.AspNetCore.Mvc;
using Raggle.Server.WebApi.DB;
using Raggle.Server.WebApi.Models;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/api/memory")]
public class MemoryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<MemoryController> _logger;

    public MemoryController(
        ILogger<MemoryController> logger,
        AppDbContext context)
    {
        _logger = logger;
        _db = context;
    }

    [HttpGet("")]
    public ActionResult<IEnumerable<Collection>> Get()
    {
        return _db.Collections;
    }
}
