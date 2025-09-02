using IronHive.Abstractions;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core.Agent;

/// <summary>
/// 채팅 에이전트의 기본 구현체입니다.
/// </summary>
public class ChatAgent : IAgent
{
    private readonly IHiveService _service;

    /// <inheritdoc />
    public required string DefaultProvider { get; set; }

    /// <inheritdoc />
    public required string DefaultModel { get; set; }

    /// <inheritdoc />
    public required string Name { get; set; }
    
    /// <inheritdoc />
    public required string Description { get; set; }

    /// <inheritdoc />
    public string? Instructions { get; set; }

    /// <inheritdoc />
    public IEnumerable<string> Tools { get; set; } = Array.Empty<string>();

    /// <inheritdoc />
    public IDictionary<string, object?> ToolOptions { get; set; } = new Dictionary<string, object?>();

    /// <inheritdoc />
    public MessageGenerationParameters? Parameters { get; set; }

    public ChatAgent(IHiveService service)
    {
        _service = service;
    }

    /// <inheritdoc />
    public Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages, 
        CancellationToken cancellationToken = default)
    {
        var generator = _service.Services.GetRequiredService<IMessageService>();
        var request = CreateRequest(messages);
        return generator.GenerateMessageAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages, 
        CancellationToken cancellationToken = default)
    {
        var generator = _service.Services.GetRequiredService<IMessageService>();
        var request = CreateRequest(messages);
        return generator.GenerateStreamingMessageAsync(request, cancellationToken);
    }

    // 만들기...
    private MessageRequest CreateRequest(IEnumerable<Message> messages) => new()
    {
        Messages = messages.ToList(),
        Provider = DefaultProvider,
        Model = DefaultModel,
        Instruction = Instructions,
        Tools = Tools,
        ToolOptions = ToolOptions,
        MaxTokens = Parameters?.MaxTokens,
        StopSequences = Parameters?.StopSequences,
        Temperature = Parameters?.Temperature,
        TopP = Parameters?.TopP,
        TopK = Parameters?.TopK,
    };
}
