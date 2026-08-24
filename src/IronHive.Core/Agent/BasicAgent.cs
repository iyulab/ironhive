using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent;

/// <summary>
/// 채팅 에이전트의 기본 구현체입니다.
/// </summary>
public class BasicAgent : IAgent
{
    private readonly IMessageService _message;

    /// <inheritdoc />
    public string Provider { get; set; } = string.Empty;

    /// <inheritdoc />
    public required string Model { get; set; }

    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public string? Instructions { get; set; }

    /// <inheritdoc />
    public IToolCollection? Tools { get; set; }

    /// <inheritdoc />
    public int? MaxTokens { get; set; }

    /// <inheritdoc cref="IronHive.Abstractions.Messages.MessageRequest.Temperature" />
    public float? Temperature { get; set; }

    /// <inheritdoc cref="IronHive.Abstractions.Messages.MessageRequest.TopP" />
    public float? TopP { get; set; }

    /// <inheritdoc cref="IronHive.Abstractions.Messages.MessageRequest.TopK" />
    public int? TopK { get; set; }

    /// <inheritdoc cref="IronHive.Abstractions.Messages.MessageRequest.StopSequences" />
    public ICollection<string>? StopSequences { get; set; }

    public BasicAgent(IMessageService service)
    {
        _message = service;
    }

    /// <inheritdoc />
    public Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        AgentInvokeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(messages, options);
        return _message.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        AgentInvokeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(messages, options);
        return _message.GenerateStreamingMessageAsync(request, cancellationToken);
    }

    private MessageRequest CreateRequest(IEnumerable<Message> messages, AgentInvokeOptions? options)
    {
        var request = new MessageRequest
        {
            Messages = messages.ToList(),
            Provider = Provider,
            Model = Model,
            System = Instructions,
            Tools = Tools,
            MaxTokens = MaxTokens,
            Temperature = Temperature,
            TopP = TopP,
            TopK = TopK,
            StopSequences = StopSequences,
        };

        if (options is null)
            return request;

        // per-request overlay — null 필드는 에이전트/요청 기본값 유지
        request.PreviousId = options.PreviousId;
        if (options.ThinkingEffort is not null) request.ThinkingEffort = options.ThinkingEffort;
        if (options.MaxTokens is not null) request.MaxTokens = options.MaxTokens;
        if (options.Temperature is not null) request.Temperature = options.Temperature;
        if (options.TopP is not null) request.TopP = options.TopP;
        if (options.TopK is not null) request.TopK = options.TopK;
        if (options.StopSequences is not null) request.StopSequences = options.StopSequences;
        if (options.Tools is not null) request.Tools = options.Tools;
        if (options.ToolOptions is not null) request.ToolOptions = options.ToolOptions;
        if (options.OutputFormat is not null) request.OutputFormat = options.OutputFormat;
        if (options.Suggestions is not null) request.Suggestions = options.Suggestions;
        if (options.MaxTurns is not null) request.MaxTurns = options.MaxTurns.Value;
        if (options.Items is not null) request.Items = options.Items;

        return request;
    }
}
