using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Raggle.Server.Entities;
using Raggle.Server.Services;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Raggle.Server.WebApi.Controllers;

[Route("/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly ConversationService _service;

    public ChatController(ConversationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> FindConversationsAsync(
        [FromQuery] int skip = 0,
        [FromQuery] int limit = 10)
    {
        var conversations = await _service.FindAsync(skip, limit);
        return Ok(conversations);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversationAsync(string id)
    {
        var conversation = await _service.FindAsync(id);
        if (conversation == null)
        {
            return NotFound();
        }
        return Ok(conversation);
    }

    [HttpPost("{id}")]
    public async Task UpsertConversationAsync(
        [FromRoute] string id,
        [FromBody] ConversationEntity entity,
        CancellationToken cancellationToken = default)
    {
        Response.ContentType = "application/stream+json";

        try
        {
            var conversation = await _service.UpsertAsync(entity);
            await foreach (var response in _service.ChatAssistantAsync(assistantId, messages, cancellationToken))
            {
                var json = JsonSerializer.Serialize(response, _jsonOptions.JsonSerializerOptions);

                var data = Encoding.UTF8.GetBytes(json + "\n");
                await Response.Body.WriteAsync(data, cancellationToken);

                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConversationAsync(
        [FromRoute] string id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
