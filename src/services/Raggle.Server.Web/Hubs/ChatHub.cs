using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Raggle.Server.API.Assistant;
using Raggle.Server.API.Repositories;
using Raggle.Server.API.Stores;
using Raggle.Server.Web.Repositories;
using System.Text;
using System.Text.Json;

namespace Raggle.Server.API.Hubs;

public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly SearchAssistant _searcher;
    private readonly DescriptionAssistant _describer;
    private readonly UserRepository _user;
    private readonly SourceRepository _source;
    private readonly ConnectionStore _con;

    public ChatHub(ILogger<ChatHub> logger, 
        SearchAssistant searcherAssistant,
        DescriptionAssistant describerAssistant,
        UserRepository userRepo,
        SourceRepository sourceRepo,
        ConnectionStore conStore)
    {
        _logger = logger;
        _searcher = searcherAssistant;
        _describer = describerAssistant;
        _user = userRepo;
        _source = sourceRepo;
        _con = conStore;
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

    public async IAsyncEnumerable<string> Chat(Guid userId, string query, IEnumerable<Guid>? sourceIds)
    {
        await foreach (var response in _searcher.AskAsync(userId, query, sourceIds))
        {
            if (response != null)
            {
                yield return response;
            }
        }
    }

    public async IAsyncEnumerable<string> Describe(string content)
    {
        await foreach (var response in _describer.Describe(content))
        {
            yield return response;
        }
    }
}
