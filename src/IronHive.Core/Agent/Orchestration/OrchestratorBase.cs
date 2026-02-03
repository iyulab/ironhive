using System.Diagnostics;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 오케스트레이터 공통 기반 클래스
/// </summary>
public abstract class OrchestratorBase : IAgentOrchestrator
{
    private readonly List<IAgent> _agents = [];

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public IReadOnlyList<IAgent> Agents => _agents.AsReadOnly();

    /// <summary>
    /// 옵션
    /// </summary>
    protected OrchestratorOptions Options { get; }

    protected OrchestratorBase(OrchestratorOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Name = options.Name ?? GetType().Name;
    }

    /// <inheritdoc />
    public void AddAgent(IAgent agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        _agents.Add(agent);
    }

    /// <inheritdoc />
    public void AddAgents(IEnumerable<IAgent> agents)
    {
        ArgumentNullException.ThrowIfNull(agents);
        _agents.AddRange(agents);
    }

    /// <inheritdoc />
    public abstract Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public abstract IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 에이전트 실행 및 결과 캡처
    /// </summary>
    protected async Task<AgentStepResult> ExecuteAgentAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var inputMessages = messages.ToList();

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.AgentTimeout);

            var response = await agent.InvokeAsync(inputMessages, cts.Token).ConfigureAwait(false);
            stopwatch.Stop();

            return new AgentStepResult
            {
                AgentName = agent.Name,
                Input = inputMessages,
                Response = response,
                Duration = stopwatch.Elapsed,
                IsSuccess = true
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return new AgentStepResult
            {
                AgentName = agent.Name,
                Input = inputMessages,
                Duration = stopwatch.Elapsed,
                IsSuccess = false,
                Error = $"Agent '{agent.Name}' timed out after {Options.AgentTimeout.TotalSeconds}s"
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AgentStepResult
            {
                AgentName = agent.Name,
                Input = inputMessages,
                Duration = stopwatch.Elapsed,
                IsSuccess = false,
                Error = $"Agent '{agent.Name}' failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 응답에서 메시지 추출
    /// </summary>
    protected static Message? ExtractMessage(MessageResponse? response)
    {
        return response?.Message;
    }

    /// <summary>
    /// 메시지에서 Content 컬렉션 추출
    /// </summary>
    protected static ICollection<MessageContent> GetMessageContent(Message? message)
    {
        return message switch
        {
            AssistantMessage assistant => assistant.Content,
            UserMessage user => user.Content,
            _ => []
        };
    }

    /// <summary>
    /// 토큰 사용량 집계
    /// </summary>
    protected static TokenUsageSummary AggregateTokenUsage(IEnumerable<AgentStepResult> steps)
    {
        var totalInput = 0;
        var totalOutput = 0;

        foreach (var step in steps)
        {
            if (step.Response?.TokenUsage != null)
            {
                totalInput += step.Response.TokenUsage.InputTokens;
                totalOutput += step.Response.TokenUsage.OutputTokens;
            }
        }

        return new TokenUsageSummary
        {
            TotalInputTokens = totalInput,
            TotalOutputTokens = totalOutput
        };
    }
}
