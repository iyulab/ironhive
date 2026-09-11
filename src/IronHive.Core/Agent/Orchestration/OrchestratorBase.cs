using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Core.Utilities;

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

    /// <inheritdoc />
    /// <remarks>
    /// 기본값은 <c>false</c>입니다. 실시간 스트리밍을 지원하는 오케스트레이터는 이 속성을 오버라이드해야 합니다.
    /// </remarks>
    public virtual bool SupportsRealTimeStreaming => false;

    /// <summary>
    /// 옵션
    /// </summary>
    protected OrchestratorOptions Options { get; }

    protected OrchestratorBase(OrchestratorOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Name = options.Name ?? GetType().Name;

        MiddlewareStreamingDiagnostics.WarnAboutBufferedOnly(Options.AgentMiddlewares, $"orchestrator '{Name}'");
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
        var inputMessages = ScopeInput(agent, messages);

        using var activity = HiveTelemetry.StartAgentActivity(agent.Name, agent.Description);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.AgentTimeout);

            var response = await InvokeWithMiddlewaresAsync(agent, inputMessages, cts.Token).ConfigureAwait(false);
            stopwatch.Stop();

            activity.SetResponseInfo(
                responseId: response.ResponseId,
                model: agent.Model,
                finishReason: response.DoneReason?.ToString(),
                inputTokens: response.TokenUsage?.InputTokens,
                outputTokens: response.TokenUsage?.OutputTokens);

            HiveTelemetry.RecordOperationDuration(
                system: agent.Provider,
                model: agent.Model,
                operationName: HiveTelemetry.Operations.AgentInvoke,
                durationSeconds: stopwatch.Elapsed.TotalSeconds,
                success: true);

            return await CompleteStepAsync(agent, inputMessages, response, stopwatch.Elapsed, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();

            HiveTelemetry.RecordOperationDuration(
                system: agent.Provider,
                model: agent.Model,
                operationName: HiveTelemetry.Operations.AgentInvoke,
                durationSeconds: stopwatch.Elapsed.TotalSeconds,
                success: false);

            var timedOut = TimedOutStep(agent, inputMessages, stopwatch.Elapsed);
            activity?.SetStatus(ActivityStatusCode.Error, timedOut.Error);
            return timedOut;
        }
        // A cancelled orchestration token is the orchestration's own timeout or the caller cancelling;
        // neither is this agent failing. Recording it as a failed step hid both: the orchestrators'
        // timeout handling never ran, and a caller's cancellation came back as a result saying
        // "A task was canceled".
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();

            activity.SetError(ex);
            HiveTelemetry.RecordOperationDuration(
                system: agent.Provider,
                model: agent.Model,
                operationName: HiveTelemetry.Operations.AgentInvoke,
                durationSeconds: stopwatch.Elapsed.TotalSeconds,
                success: false);

            return FailedStep(agent, inputMessages, stopwatch.Elapsed, ex);
        }
    }

    /// <summary>
    /// 에이전트를 스트리밍으로 실행하고 <see cref="ExecuteAgentAsync"/>와 같은 규칙으로 단계 결과를 만듭니다 —
    /// 컨텍스트 스코프, 결과 증류, 오류 문구, 타임아웃 구분까지. 받은 청크는 <paramref name="onChunk"/>로 넘깁니다.
    /// </summary>
    /// <remarks>
    /// 실시간 스트리밍 오케스트레이터가 단계를 각자 재구성하던 자리다(CONVENTIONS §5): 텍스트 델타만 모아 추론·도구
    /// 콘텐츠를 버렸고, 스코프와 증류를 건너뛰었고, 타임아웃 문구가 달랐다. 응답 메시지는 에이전트가 done 프레임에
    /// 실어 보낸 것(에이전트 경로의 MessageService가 채운다)을 쓰고, 텍스트 누적은 그것을 싣지 않는 생산자를 위한
    /// 대체다. 오케스트레이션 토큰이 취소되면 예외를 그대로 던진다 — 호출한 오케스트레이터가 버퍼드와 같이 처리한다.
    /// </remarks>
    private protected async Task<AgentStepResult> ExecuteAgentStreamingStepAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        Func<StreamingMessageResponse, ValueTask> onChunk,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var inputMessages = ScopeInput(agent, messages);
        var text = new StringBuilder();
        StreamingMessageDoneResponse? done = null;

        try
        {
            await foreach (var chunk in ExecuteAgentStreamingAsync(agent, inputMessages, cancellationToken).ConfigureAwait(false))
            {
                await onChunk(chunk).ConfigureAwait(false);

                switch (chunk)
                {
                    case StreamingContentDeltaResponse { Delta: TextDeltaContent textDelta }:
                        text.Append(textDelta.Value);
                        break;
                    case StreamingMessageDoneResponse doneFrame:
                        done = doneFrame;
                        break;
                }
            }

            stopwatch.Stop();

            var response = new MessageResponse
            {
                ResponseId = done?.ResponseId,
                DoneReason = done?.DoneReason,
                Message = done?.Message ?? new Message
                {
                    Role = MessageRole.Assistant,
                    Content = [new TextMessageContent { Value = text.ToString() }]
                },
                TokenUsage = done?.TokenUsage,
                Model = done?.Model ?? string.Empty,
                Timestamp = done?.Timestamp ?? DateTime.UtcNow
            };

            return await CompleteStepAsync(agent, inputMessages, response, stopwatch.Elapsed, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return TimedOutStep(agent, inputMessages, stopwatch.Elapsed);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            return FailedStep(agent, inputMessages, stopwatch.Elapsed, ex);
        }
    }

    /// <summary>
    /// 실패로 끝난 실시간 스트림의 종결 이벤트 — 결과를 함께 실어 버퍼드 호출과 대조할 수 있게 한다.
    /// </summary>
    private protected static ValueTask WriteFailedAsync(
        ChannelWriter<OrchestrationStreamEvent> writer,
        string error,
        List<AgentStepResult> steps,
        TimeSpan duration,
        CancellationToken cancellationToken)
        => writer.WriteAsync(new OrchestrationStreamEvent
        {
            EventType = OrchestrationEventType.Failed,
            Error = error,
            Result = OrchestrationResult.Failure(error, steps, duration)
        }, cancellationToken);

    private List<Message> ScopeInput(IAgent agent, IEnumerable<Message> messages)
    {
        var inputMessages = messages.ToList();

        // Apply context scoping if configured
        if (Options.ContextScope is null)
        {
            return inputMessages;
        }

        var scoped = Options.ContextScope.ScopeMessages(inputMessages, agent.Name);
        return scoped is List<Message> list ? list : [.. scoped];
    }

    private async Task<AgentStepResult> CompleteStepAsync(
        IAgent agent,
        List<Message> inputMessages,
        MessageResponse response,
        TimeSpan duration,
        CancellationToken cancellationToken)
    {
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
            Duration = duration,
            IsSuccess = true
        };
    }

    private AgentStepResult TimedOutStep(IAgent agent, List<Message> inputMessages, TimeSpan duration) => new()
    {
        AgentName = agent.Name,
        Input = inputMessages,
        Duration = duration,
        IsSuccess = false,
        Error = $"Agent '{agent.Name}' timed out after {Options.AgentTimeout.TotalSeconds}s"
    };

    private static AgentStepResult FailedStep(IAgent agent, List<Message> inputMessages, TimeSpan duration, Exception ex) => new()
    {
        AgentName = agent.Name,
        Input = inputMessages,
        Duration = duration,
        IsSuccess = false,
        Error = $"Agent '{agent.Name}' failed: {ex.Message}"
    };

    /// <summary>
    /// 에이전트를 스트리밍 방식으로 실행하고 결과를 캡처합니다. (OpenTelemetry 추적 포함)
    /// </summary>
    protected async IAsyncEnumerable<StreamingMessageResponse> ExecuteAgentStreamingAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var activity = HiveTelemetry.StartAgentActivity(agent.Name, agent.Description);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.AgentTimeout);

        await foreach (var chunk in InvokeStreamingWithMiddlewaresAsync(agent, messages, cts.Token).ConfigureAwait(false))
        {
            yield return chunk;
        }

        HiveTelemetry.RecordOperationDuration(
            system: agent.Provider,
            model: agent.Model,
            operationName: HiveTelemetry.Operations.AgentInvoke,
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
        return message?.Content ?? [];
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
    /// <see cref="ExecuteAsync"/>를 호출한 후 결과를 스트리밍 이벤트 시퀀스로 래핑합니다.
    /// <see cref="SupportsRealTimeStreaming"/>이 <c>false</c>인 오케스트레이터에서 사용합니다.
    /// 실시간 <see cref="OrchestrationEventType.MessageDelta"/> 이벤트는 생성되지 않습니다.
    /// </summary>
    protected async IAsyncEnumerable<OrchestrationStreamEvent> WrapAsStreamAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(messages, cancellationToken).ConfigureAwait(false);

        yield return new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started };

        foreach (var step in result.Steps)
        {
            yield return new OrchestrationStreamEvent
            {
                EventType = step.IsSuccess
                    ? OrchestrationEventType.AgentCompleted
                    : OrchestrationEventType.AgentFailed,
                AgentName = step.AgentName,
                CompletedResponse = step.Response,
                Error = step.Error
            };
        }

        yield return new OrchestrationStreamEvent
        {
            EventType = result.IsSuccess
                ? OrchestrationEventType.Completed
                : OrchestrationEventType.Failed,
            Result = result,
            Error = result.Error
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
            return await agent.InvokeAsync(messages, options: null, cancellationToken).ConfigureAwait(false);
        }

        // 미들웨어 체인 구성 (오케스트레이터는 per-request 옵션을 사용하지 않음 — 멤버 에이전트 구성으로 대체)
        Func<IEnumerable<Message>, AgentInvokeOptions?, Task<MessageResponse>> pipeline =
            (msgs, opts) => agent.InvokeAsync(msgs, opts, cancellationToken);

        for (var i = middlewares.Count - 1; i >= 0; i--)
        {
            var middleware = middlewares[i];
            var next = pipeline;
            pipeline = (msgs, opts) => middleware.InvokeAsync(agent, msgs, opts, next, cancellationToken);
        }

        return await pipeline(messages, null).ConfigureAwait(false);
    }

    private IAsyncEnumerable<StreamingMessageResponse> InvokeStreamingWithMiddlewaresAsync(
        IAgent agent,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var streamingMiddlewares = Options.AgentMiddlewares?
            .OfType<IStreamingAgentMiddleware>()
            .ToList();

        if (streamingMiddlewares == null || streamingMiddlewares.Count == 0)
        {
            return agent.InvokeStreamingAsync(messages, options: null, cancellationToken);
        }

        // 스트리밍 미들웨어 체인 구성: 마지막 미들웨어부터 역순으로 래핑
        Func<IEnumerable<Message>, AgentInvokeOptions?, IAsyncEnumerable<StreamingMessageResponse>> pipeline =
            (msgs, opts) => agent.InvokeStreamingAsync(msgs, opts, cancellationToken);

        for (var i = streamingMiddlewares.Count - 1; i >= 0; i--)
        {
            var middleware = streamingMiddlewares[i];
            var next = pipeline;
            pipeline = (msgs, opts) => middleware.InvokeStreamingAsync(agent, msgs, opts, next, cancellationToken);
        }

        return pipeline(messages, null);
    }
}
