using Microsoft.AspNetCore.SignalR;
using Raggle.Server.API.Assistant;
using Raggle.Server.API.Repositories;
using Raggle.Server.API.Stores;
using System.Text;

namespace Raggle.Server.API.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly SearchAssistant _bot;
    private readonly UserRepository _user;
    private readonly ConnectionStore _con;

    public ChatHub(ILogger<ChatHub> logger, SearchAssistant bot, UserRepository user, ConnectionStore con)
    {
        _logger = logger;
        _bot = bot;
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
        var user = await _user.GetUser(userId);
        user.ChatHistory.AddUserMessage(query);
        var answer = new StringBuilder();
        await foreach (var response in _bot.Search(user.ChatHistory))
        {
            if (response != null)
            {
                answer.Append(response);
                yield return response;
            }
        }
        user.ChatHistory.AddAssistantMessage(answer.ToString());
        _user.UpdateUser(user);
        Console.WriteLine("Chatting...");
    }
}
