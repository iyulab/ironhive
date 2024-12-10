using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Messages;
using Raggle.Server.Entities;
using Raggle.Server.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Raggle.Server.WebApi.Controllers;

[ApiController]
[Route("/v1/assistants")]
public class AssistantController : ControllerBase
{
    private readonly ILogger<AssistantController> _logger;
    private readonly JsonOptions _jsonOptions;
    private readonly AssistantService _service;

    public AssistantController(
        ILogger<AssistantController> logger,
        IOptions<JsonOptions> jsonOptions,
        AssistantService service)
    {
        _logger = logger;
        _jsonOptions = jsonOptions.Value;
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

    [HttpGet("{assistantId}")]
    public async Task<ActionResult> FindAssistantAsync(
        [FromRoute] string assistantId)
    {
        var assistant = await _service.GetAssistantAsync(assistantId);
        return assistant != null ? Ok(assistant) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult> UpsertAssistantAsync(
        [FromBody] AssistantEntity assistant)
    {
        var result = await _service.UpsertAssistantAsync(assistant);
        return Ok(result);
    }

    [HttpDelete("{assistantId}")]
    public async Task<ActionResult> DeleteAssistantAsync(
        [FromRoute] string assistantId)
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

    [HttpPost("{assistantId}/chat")]
    public async Task ChatAssistantAsync(
        [FromRoute] string assistantId,
        [FromBody] MessageCollection messages,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "application/stream+json";

        try
        {
            await foreach (var response in _service.ChatAssistantAsync(assistantId, messages, cancellationToken))
            {
                var json = JsonSerializer.Serialize(response, _jsonOptions.JsonSerializerOptions);

                var data = Encoding.UTF8.GetBytes(json + "\n");
                await Response.Body.WriteAsync(data, cancellationToken);

                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
        }
    }
}
