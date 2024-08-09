using Microsoft.AspNetCore.SignalR;
using Raggle.Server.API.Assistant;
using Raggle.Server.API.Repositories;
using Raggle.Server.API.Stores;
using System.Text;
using System.Text.Json;

namespace Raggle.Server.API.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly SearchAssistant _searcher;
    private readonly DescriptionAssistant _describer;
    private readonly UserRepository _user;
    private readonly ConnectionStore _con;

    public ChatHub(ILogger<ChatHub> logger, 
        SearchAssistant searcher,
        DescriptionAssistant describer,
        UserRepository user, 
        ConnectionStore con)
    {
        _logger = logger;
        _searcher = searcher;
        _describer = describer;
        _user = user;
        _con = con;
    }

    public override Task OnConnectedAsync()
    {
        if (Context.GetHttpContext().Request.Query.TryGetValue("userid", out var values))
        {
            _con.Set(Context.ConnectionId, values.First());
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _con.Remove(Context.ConnectionId).ConfigureAwait(false);
        return base.OnDisconnectedAsync(exception);
    }

    public async IAsyncEnumerable<string> Chat(Guid userId, string query)
    {
        var user = await _user.GetAsync(userId);
        user.ChatHistory.AddUserMessage(query);
        var answer = new StringBuilder();
        await foreach (var response in _searcher.Search(user.ChatHistory))
        {
            if (response != null)
            {
                answer.Append(response);
                yield return response;
            }
        }
        user.ChatHistory.AddAssistantMessage(answer.ToString());
        await _user.UpdateAsync(user.ID, JsonSerializer.SerializeToElement(new
        {
            ChatHistory = user.ChatHistory
        }));
    }

    public async IAsyncEnumerable<string> Describe(string content)
    {
        await foreach (var response in _describer.Describe(content))
        {
            yield return response;
        }
    }
}
