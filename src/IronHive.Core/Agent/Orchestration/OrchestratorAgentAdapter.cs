using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 오케스트레이터를 IAgent로 래핑하여 중첩 오케스트레이션을 가능하게 합니다.
/// </summary>
/// <remarks>
/// per-request <see cref="AgentInvokeOptions"/>는 지원하지 않습니다 —
/// 멤버 에이전트/오케스트레이터 옵션으로 구성하세요.
/// </remarks>
public class OrchestratorAgentAdapter : IAgent
{
    private readonly IAgentOrchestrator _orchestrator;

    public string Provider { get; set; } = "orchestrator";
    public string Model { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string? Instructions { get; set; }
    public IToolCollection? Tools { get; set; }
    public int? MaxTokens { get; set; }

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
        AgentInvokeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfOptionsProvided(options);

        var result = await _orchestrator.ExecuteAsync(messages, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Orchestrator '{Name}' failed: {result.Error}");
        }

        // OrchestrationResult.FinalOutput → MessageResponse로 변환
        var outputMessage = result.FinalOutput as Message
            ?? ConvertToAssistantMessage(result.FinalOutput);

        return new MessageResponse
        {
            ResponseId = null,
            DoneReason = MessageDoneReason.EndTurn,
            Message = outputMessage,
            TokenUsage = result.TokenUsage != null
                ? new MessageTokenUsage
                {
                    InputTokens = result.TokenUsage.TotalInputTokens,
                    OutputTokens = result.TokenUsage.TotalOutputTokens
                }
                : null,
            Model = string.Empty,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingAsync(
        IEnumerable<Message> messages,
        AgentInvokeOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // iterator 밖에서 검사하여 열거 시작 전 즉시 throw (fail-loud)
        ThrowIfOptionsProvided(options);

        return InvokeStreamingCoreAsync(messages, cancellationToken);
    }

    private async IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingCoreAsync(
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

    private static void ThrowIfOptionsProvided(AgentInvokeOptions? options)
    {
        if (options is not null)
        {
            throw new NotSupportedException(
                "Orchestrator-wrapped agents do not support per-request AgentInvokeOptions. " +
                "Configure member agents or orchestrator options instead.");
        }
    }

    private static Message ConvertToAssistantMessage(Message? message)
    {
        if (message is { Role: MessageRole.User })
            return new Message { Role = MessageRole.Assistant, Content = [.. message.Content] };

        return new Message { Role = MessageRole.Assistant,
            Content = [new TextMessageContent { Value = string.Empty }]
        };
    }
}
