using Microsoft.AspNetCore.Mvc;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/api/document")]
public class DocumentController : ControllerBase
{
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(ILogger<DocumentController> logger)
    {
        _logger = logger;
    }

    [HttpGet("collections")]
    public ActionResult<string> Get()
    {
        return "DocumentController";
    }

    [HttpGet("collections/{collectionId}")]
    public ActionResult<string> Get(int collectionId)
    {
        return $"DocumentController {collectionId}";
    }

    [HttpPost("collections")]
    public ActionResult<string> Post()
    {
        return "DocumentController";
    }

    [HttpPut("collections/{collectionId}")]
    public ActionResult<string> Put(int collectionId)
    {
        return $"DocumentController {collectionId}";
    }

    [HttpDelete("collections/{collectionId}")]
    public ActionResult<string> Delete(int collectionId)
    {
        return $"DocumentController {collectionId}";
    }
}
