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
    public required string Provider { get; set; }

    /// <inheritdoc />
    public required string Model { get; set; }

    /// <inheritdoc />
    public required string Name { get; set; }
    
    /// <inheritdoc />
    public required string Description { get; set; }

    /// <inheritdoc />
    public string? SystemPrompt { get; set; }

    /// <inheritdoc />
    public IEnumerable<ToolItem>? Tools { get; set; }

    /// <inheritdoc />
    public MessageGenerationParameters? Parameters { get; set; }

    public BasicAgent(IMessageService service)
    {
        _message = service;
    }

    /// <inheritdoc />
    public Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages, 
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(messages);
        return _message.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages, 
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(messages);
        return _message.GenerateStreamingMessageAsync(request, cancellationToken);
    }

    // 만들기...
    private MessageRequest CreateRequest(IEnumerable<Message> messages) => new()
    {
        Messages = messages.ToList(),
        Provider = Provider,
        Model = Model,
        SystemPrompt = SystemPrompt,
        Tools = Tools ?? [],
        MaxTokens = Parameters?.MaxTokens,
        StopSequences = Parameters?.StopSequences,
        Temperature = Parameters?.Temperature,
        TopP = Parameters?.TopP,
        TopK = Parameters?.TopK,
    };
}
