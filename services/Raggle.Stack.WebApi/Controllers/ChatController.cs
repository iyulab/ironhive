using Microsoft.AspNetCore.Mvc;
using Raggle.Stack.EFCore.Entities;
using Raggle.Stack.WebApi.Services;
using System.Diagnostics;

namespace Raggle.Stack.WebApi.Controllers;

[Route("/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly SessionService _service;

    public ChatController(SessionService service)
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
        [FromBody] SessionEntity entity,
        CancellationToken cancellationToken = default)
    {
        Response.ContentType = "application/stream+json";

        try
        {
            var conversation = await _service.UpsertAsync(entity);
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
