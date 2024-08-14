using Microsoft.AspNetCore.SignalR;
using Raggle.Server.Web.Services;
using Raggle.Server.Web.Stores;

namespace Raggle.Server.Web.Hubs;

public class AppHub : Hub
{
    private readonly ConnectionStore _con;
    private readonly UserAssistantService _assistant;
    private readonly ChatGenerateService _chat;

    public AppHub(
        ConnectionStore connectionStore, 
        UserAssistantService assistantService,
        ChatGenerateService chatService)
    {
        _con = connectionStore;
        _assistant = assistantService;
        _chat = chatService;
    }

    public async override Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await _con.SetAsync(userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await _con.RemoveAsync(userId);
        await base.OnDisconnectedAsync(exception);
    }

    public async IAsyncEnumerable<string> Chat(Guid assistantId, string query, IEnumerable<string>? tags)
    {
        await foreach (var response in _assistant.AskAsync(assistantId, query, tags))
        {
            if (response != null)
            {
                yield return response;
            }
        }
    }

    public async IAsyncEnumerable<string> Explain(string content)
    {
        await foreach (var response in _chat.ExplainAsync(content))
        {
            yield return response.ToString();
        }
    }

    private Guid GetUserId()
    {
        var userIdStr = Context.GetHttpContext()?.Request.Query["userid"].FirstOrDefault();
        return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
    }
}
