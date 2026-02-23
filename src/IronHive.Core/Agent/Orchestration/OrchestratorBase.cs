using System.Diagnostics;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Telemetry;

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
    /// 오케스트레이션 ID를 가져옵니다. 옵션에 지정되지 않았으면 자동 생성합니다.
    /// </summary>
    protected string GetOrchestrationId()
    {
        return Options.OrchestrationId ??= Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 체크포인트를 저장합니다.
    /// </summary>
    protected async Task SaveCheckpointAsync(
        List<AgentStepResult> completedSteps,
        IReadOnlyList<Message> currentMessages,
        CancellationToken ct)
    {
        if (Options.CheckpointStore == null) return;

        var orchestrationId = GetOrchestrationId();
        var checkpoint = new OrchestrationCheckpoint
        {
            OrchestrationId = orchestrationId,
            OrchestratorName = Name,
            CompletedStepCount = completedSteps.Count,
            CompletedSteps = completedSteps.ToList(),
            CurrentMessages = currentMessages.ToList()
        };

        await Options.CheckpointStore.SaveAsync(orchestrationId, checkpoint, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// 체크포인트를 로드합니다.
    /// </summary>
    protected async Task<OrchestrationCheckpoint?> LoadCheckpointAsync(CancellationToken ct)
    {
        if (Options.CheckpointStore == null) return null;

        var orchestrationId = GetOrchestrationId();
        return await Options.CheckpointStore.LoadAsync(orchestrationId, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// 체크포인트를 삭제합니다.
    /// </summary>
    protected async Task DeleteCheckpointAsync(CancellationToken ct)
    {
        if (Options.CheckpointStore == null) return;

        var orchestrationId = GetOrchestrationId();
        await Options.CheckpointStore.DeleteAsync(orchestrationId, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// 에이전트 실행 전 승인을 확인합니다.
    /// ApprovalHandler가 없으면 항상 true를 반환합니다.
    /// </summary>
    protected async Task<bool> CheckApprovalAsync(
        IAgent agent,
        AgentStepResult? previousStep,
        CancellationToken ct)
    {
        if (Options.ApprovalHandler == null) return true;

        // RequireApprovalForAgents가 지정된 경우 해당 에이전트만 체크
        if (Options.RequireApprovalForAgents != null &&
            !Options.RequireApprovalForAgents.Contains(agent.Name))
        {
            return true;
        }

        return await Options.ApprovalHandler(agent.Name, previousStep).ConfigureAwait(false);
    }

    /// <summary>
    /// 에이전트 실행 및 결과 캡처 (OpenTelemetry 추적 포함)
    /// </summary>
    protected async Task<AgentStepResult> ExecuteAgentAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var inputMessages = messages.ToList();

        // Apply context scoping if configured
        if (Options.ContextScope is not null)
        {
            var scoped = Options.ContextScope.ScopeMessages(inputMessages, agent.Name);
            inputMessages = scoped is List<Message> list ? list : [.. scoped];
        }

        using var activity = IronHiveTelemetry.StartAgentActivity(agent.Name, agent.Description);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.AgentTimeout);

            var response = await InvokeWithMiddlewaresAsync(agent, inputMessages, cts.Token).ConfigureAwait(false);
            stopwatch.Stop();

            activity.SetResponseInfo(
                responseId: response.Id,
                model: agent.Model,
                finishReason: response.DoneReason?.ToString(),
                inputTokens: response.TokenUsage?.InputTokens,
                outputTokens: response.TokenUsage?.OutputTokens);

            IronHiveTelemetry.RecordOperationDuration(
                system: agent.Provider,
                model: agent.Model,
                operationName: IronHiveTelemetry.Operations.AgentInvoke,
                durationSeconds: stopwatch.Elapsed.TotalSeconds,
                success: true);

            // Apply result distillation if configured
            if (Options.ResultDistiller is not null)
            {
                response = await Options.ResultDistiller.DistillAsync(
                    agent.Name, response, Options.ResultDistillationOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

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

            IronHiveTelemetry.RecordOperationDuration(
                system: agent.Provider,
                model: agent.Model,
                operationName: IronHiveTelemetry.Operations.AgentInvoke,
                durationSeconds: stopwatch.Elapsed.TotalSeconds,
                success: false);

            var errorMessage = $"Agent '{agent.Name}' timed out after {Options.AgentTimeout.TotalSeconds}s";
            activity?.SetStatus(ActivityStatusCode.Error, errorMessage);

            return new AgentStepResult
            {
                AgentName = agent.Name,
                Input = inputMessages,
                Duration = stopwatch.Elapsed,
                IsSuccess = false,
                Error = errorMessage
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            activity.SetError(ex);
            IronHiveTelemetry.RecordOperationDuration(
                system: agent.Provider,
                model: agent.Model,
                operationName: IronHiveTelemetry.Operations.AgentInvoke,
                durationSeconds: stopwatch.Elapsed.TotalSeconds,
                success: false);

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
    /// 에이전트를 스트리밍 방식으로 실행하고 결과를 캡처합니다. (OpenTelemetry 추적 포함)
    /// </summary>
    protected async IAsyncEnumerable<StreamingMessageResponse> ExecuteAgentStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var activity = IronHiveTelemetry.StartAgentActivity(agent.Name, agent.Description);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.AgentTimeout);

        await foreach (var chunk in agent.InvokeStreamingAsync(messages, cts.Token).ConfigureAwait(false))
        {
            yield return chunk;
        }

        IronHiveTelemetry.RecordOperationDuration(
            system: agent.Provider,
            model: agent.Model,
            operationName: IronHiveTelemetry.Operations.AgentInvoke,
            durationSeconds: 0, // 스트리밍은 정확한 시간 측정이 어려움
            success: true);
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

    /// <summary>
    /// 미들웨어 체인을 거쳐 에이전트를 실행합니다.
    /// </summary>
    private async Task<MessageResponse> InvokeWithMiddlewaresAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var middlewares = Options.AgentMiddlewares;
        if (middlewares == null || middlewares.Count == 0)
        {
            return await agent.InvokeAsync(messages, cancellationToken).ConfigureAwait(false);
        }

        // 미들웨어 체인 구성
        Func<IEnumerable<Message>, Task<MessageResponse>> pipeline =
            msgs => agent.InvokeAsync(msgs, cancellationToken);

        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var next = pipeline;
            pipeline = msgs => middleware.InvokeAsync(agent, msgs, next, cancellationToken);
        }

        return await pipeline(messages).ConfigureAwait(false);
    }
}
