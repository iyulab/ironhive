using IronHive.Abstractions;
using IronHive.Abstractions.ChatCompletion;
using IronHive.Abstractions.Messages;
using System.Runtime.CompilerServices;

namespace IronHive.Core;

public class HiveSession : IHiveSession
{
    private readonly IChatCompletionService _serivce;
    private readonly IHiveAgent _master;
    private readonly Dictionary<string, IHiveAgent> _agents;

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public int? LastSummarizedIndex { get; set; }

    public MessageCollection Messages { get; set; } = new();

    public int? TotalTokens { get; set; }

    public int MaxToolAttempts { get; set; } = 3;

    public HiveSession(IChatCompletionService service,
        IHiveAgent master,
        IEnumerable<IHiveAgent>? agents = null)
    {
        _serivce = service;
        _master = master;
        _agents = agents?.ToDictionary(a => a.Name, a => a)
            ?? new Dictionary<string, IHiveAgent>();
    }

    public Task<IMessage> InvokeAsync(
        string text,
        IEnumerable<string> files,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var message = new UserMessage();
        message.Content.Add(new UserTextContent { Value = text });
        return _serivce.GenerateMessageAsync(Messages, options, cancellationToken);
    }

    public IAsyncEnumerable<IAssistantContent> InvokeStreamingAsync(
        string text,
        IEnumerable<string>? files,
        ChatCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        var message = new UserMessage();
        message.Content.Add(new UserTextContent { Value = text });
        return _serivce.GenerateStreamingMessageAsync(Messages, options, cancellationToken);
    }

    public void AddAgent(IHiveAgent agent)
    {
        _agents.Add(agent.Name, agent);
    }

    public void RemoveAgent(string name)
    {
        _agents.Remove(name);
    }
}
