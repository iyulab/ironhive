using Microsoft.AspNetCore.Mvc;
using Raggle.Server.WebApi.Models;
using Raggle.Server.WebApi.Services;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/v1/assistants")]
public class AssistantController : ControllerBase
{
    private readonly ILogger<AssistantController> _logger;
    private readonly AssistantService _service;

    public AssistantController(
        ILogger<AssistantController> logger,
        AssistantService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult> GetAssistantsAsync(
        [FromQuery(Name = "skip")] int skip = 0,
        [FromQuery(Name = "limit")] int limit = 10)
    {
        var assistants = await _service.GetAssistantsAsync(skip, limit);
        return assistants.Count() > 0 ? Ok(assistants) : NoContent();
    }

    [HttpGet("{assistantId:guid}")]
    public async Task<ActionResult> FindAssistantAsync(
        [FromRoute] Guid assistantId)
    {
        var assistant = await _service.GetAssistantAsync(assistantId);
        return assistant != null ? Ok(assistant) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> UpsertAssistantAsync(
        [FromBody] AssistantModel assistant)
    {
        var result = await _service.UpsertAssistantAsync(assistant);
        return Ok(result);
    }

    [HttpDelete("{assistantId:guid}")]
    public async Task<ActionResult> DeleteAssistantAsync(
        [FromRoute] Guid assistantId)
    {
        try
        {
            await _service.DeleteAssistantAsync(assistantId);
            return Ok($"Deleted: {assistantId}");
        }
        catch(KeyNotFoundException)
        {
            return BadRequest("Assistant not found");
        }
    }
}
