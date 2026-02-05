using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 오케스트레이터를 IAgent로 래핑하여 중첩 오케스트레이션을 가능하게 합니다.
/// </summary>
public class OrchestratorAgentAdapter : IAgent
{
    private readonly IAgentOrchestrator _orchestrator;

    public string Provider { get; set; } = "orchestrator";
    public string Model { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Instructions { get; set; }
    public IEnumerable<ToolItem>? Tools { get; set; }
    public MessageGenerationParameters? Parameters { get; set; }

    public OrchestratorAgentAdapter(IAgentOrchestrator orchestrator, string? name = null, string? description = null)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);
        _orchestrator = orchestrator;

        Model = orchestrator.GetType().Name;
        Name = name ?? orchestrator.Name;
        Description = description ?? $"Orchestrator '{orchestrator.Name}' as agent";
    }

    /// <inheritdoc />
    public async Task<MessageResponse> InvokeAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        var result = await _orchestrator.ExecuteAsync(messages, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Orchestrator '{Name}' failed: {result.Error}");
        }

        // OrchestrationResult.FinalOutput → MessageResponse로 변환
        var outputMessage = result.FinalOutput as AssistantMessage
            ?? ConvertToAssistantMessage(result.FinalOutput);

        return new MessageResponse
        {
            Id = Guid.NewGuid().ToString("N"),
            DoneReason = MessageDoneReason.EndTurn,
            Message = outputMessage,
            TokenUsage = result.TokenUsage != null
                ? new MessageTokenUsage
                {
                    InputTokens = result.TokenUsage.TotalInputTokens,
                    OutputTokens = result.TokenUsage.TotalOutputTokens
                }
                : null
        };
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var evt in _orchestrator.ExecuteStreamingAsync(messages, cancellationToken).ConfigureAwait(false))
        {
            // MessageDelta 이벤트만 forward
            if (evt.EventType == OrchestrationEventType.MessageDelta && evt.StreamingResponse != null)
            {
                yield return evt.StreamingResponse;
            }
        }
    }

    private AssistantMessage ConvertToAssistantMessage(Message? message)
    {
        if (message == null)
        {
            return new AssistantMessage
            {
                Name = Name,
                Content = [new TextMessageContent { Value = string.Empty }]
            };
        }

        if (message is UserMessage userMsg)
        {
            return new AssistantMessage
            {
                Name = Name,
                Content = [.. userMsg.Content]
            };
        }

        // fallback
        return new AssistantMessage
        {
            Name = Name,
            Content = [new TextMessageContent { Value = string.Empty }]
        };
    }
}
