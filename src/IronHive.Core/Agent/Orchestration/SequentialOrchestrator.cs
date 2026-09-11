using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 에이전트를 순차적으로 실행하는 오케스트레이터입니다.
/// 각 에이전트의 출력이 다음 에이전트의 입력으로 전달됩니다.
/// </summary>
public class SequentialOrchestrator : OrchestratorBase
{
    private new SequentialOrchestratorOptions Options => (SequentialOrchestratorOptions)base.Options;

    /// <inheritdoc />
    public override bool SupportsRealTimeStreaming => true;

    public SequentialOrchestrator(SequentialOrchestratorOptions? options = null)
        : base(options ?? new SequentialOrchestratorOptions())
    {
    }

    /// <inheritdoc />
    public override async Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (!Agents.Any())
        {
            return OrchestrationResult.Failure("No agents registered in the orchestrator.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();
        var currentMessages = messages.ToList();
        var accumulatedMessages = new List<Message>(currentMessages);
        var startIndex = 0;

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        try
        {
            // 체크포인트에서 재개
            var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
            if (checkpoint != null)
            {
                steps.AddRange(checkpoint.CompletedSteps);
                startIndex = checkpoint.CompletedStepCount;
                currentMessages = checkpoint.CurrentMessages.ToList();
                accumulatedMessages = new List<Message>(currentMessages);
            }

            var agentList = Agents.ToList();
            for (var i = startIndex; i < agentList.Count; i++)
            {
                var agent = agentList[i];

                // 승인 체크
                var previousStep = steps.LastOrDefault();
                if (!await CheckApprovalAsync(agent, previousStep, cts.Token).ConfigureAwait(false))
                {
                    await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(
                        ApprovalDenied(agent),
                        steps,
                        stopwatch.Elapsed);
                }

                var input = Options.AccumulateHistory ? accumulatedMessages : currentMessages;
                var stepResult = await ExecuteAgentAsync(agent, input, cts.Token).ConfigureAwait(false);
                steps.Add(stepResult);

                if (!stepResult.IsSuccess)
                {
                    if (Options.StopOnAgentFailure)
                    {
                        await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                        stopwatch.Stop();
                        return OrchestrationResult.Failure(
                            FailureOf(stepResult),
                            steps,
                            stopwatch.Elapsed);
                    }
                    continue;
                }

                // 다음 에이전트를 위한 입력 준비
                PassOutputForward(stepResult, ref currentMessages, accumulatedMessages);

                // 각 단계 완료 후 체크포인트 저장
                await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
            }

            stopwatch.Stop();

            // 마지막 성공 응답을 최종 출력으로 사용
            var finalOutput = ExtractMessage(steps.LastOrDefault(s => s.IsSuccess)?.Response);

            if (finalOutput == null)
            {
                return OrchestrationResult.Failure(
                    NoSuccessfulOutput,
                    steps,
                    stopwatch.Elapsed);
            }

            // 완료 시 체크포인트 삭제
            await DeleteCheckpointAsync(cancellationToken).ConfigureAwait(false);

            return OrchestrationResult.Success(
                finalOutput,
                steps,
                stopwatch.Elapsed,
                AggregateTokenUsage(steps));
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                OrchestrationTimedOut(),
                steps,
                stopwatch.Elapsed);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!Agents.Any())
        {
            yield return new OrchestrationStreamEvent
            {
                EventType = OrchestrationEventType.Failed,
                Error = "No agents registered in the orchestrator."
            };
            yield break;
        }

        // Channel을 사용하여 try-catch 내부의 yield 제한을 우회
        var channel = Channel.CreateUnbounded<OrchestrationStreamEvent>();

        var producerTask = ProduceStreamingEventsAsync(channel.Writer, messages, cancellationToken);

        await foreach (var streamEvent in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return streamEvent;
        }

        // producer가 완료된 후 예외 전파
        await producerTask.ConfigureAwait(false);
    }

    // The same run as ExecuteAsync, step for step: each agent goes through the shared streaming step
    // (ExecuteAgentStreamingStepAsync), a failure under StopOnAgentFailure ends the run as a failure,
    // and an orchestration timeout ends it without recording the interrupted agent as a step.
    private async Task ProduceStreamingEventsAsync(
        ChannelWriter<OrchestrationStreamEvent> writer,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();

        try
        {
            var currentMessages = messages.ToList();
            var accumulatedMessages = new List<Message>(currentMessages);
            var startIndex = 0;

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.Timeout);

            try
            {
                // 체크포인트에서 재개
                var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
                if (checkpoint != null)
                {
                    steps.AddRange(checkpoint.CompletedSteps);
                    startIndex = checkpoint.CompletedStepCount;
                    currentMessages = checkpoint.CurrentMessages.ToList();
                    accumulatedMessages = new List<Message>(currentMessages);
                }

                await writer.WriteAsync(
                    new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started },
                    cancellationToken).ConfigureAwait(false);

                var agentList = Agents.ToList();
                for (var i = startIndex; i < agentList.Count; i++)
                {
                    var agent = agentList[i];

                    // 승인 체크
                    if (!await CheckApprovalAsync(agent, steps.LastOrDefault(), cts.Token).ConfigureAwait(false))
                    {
                        await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);

                        await writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.ApprovalDenied,
                            AgentName = agent.Name,
                            Error = ApprovalDenied(agent)
                        }, cancellationToken).ConfigureAwait(false);

                        stopwatch.Stop();
                        await WriteFailedAsync(writer, ApprovalDenied(agent), steps, stopwatch.Elapsed, cancellationToken)
                            .ConfigureAwait(false);
                        return;
                    }

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentStarted,
                        AgentName = agent.Name
                    }, cancellationToken).ConfigureAwait(false);

                    var input = Options.AccumulateHistory ? accumulatedMessages : currentMessages;
                    var stepResult = await ExecuteAgentStreamingStepAsync(
                        agent,
                        input,
                        // 실시간으로 MessageDelta 이벤트 발행
                        chunk => writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.MessageDelta,
                            AgentName = agent.Name,
                            StreamingResponse = chunk
                        }, cancellationToken),
                        cts.Token).ConfigureAwait(false);
                    steps.Add(stepResult);

                    if (!stepResult.IsSuccess)
                    {
                        await writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.AgentFailed,
                            AgentName = agent.Name,
                            Error = stepResult.Error
                        }, cancellationToken).ConfigureAwait(false);

                        if (Options.StopOnAgentFailure)
                        {
                            await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                            stopwatch.Stop();
                            await WriteFailedAsync(writer, FailureOf(stepResult), steps, stopwatch.Elapsed, cancellationToken)
                                .ConfigureAwait(false);
                            return;
                        }
                        continue;
                    }

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentCompleted,
                        AgentName = agent.Name,
                        CompletedResponse = stepResult.Response
                    }, cancellationToken).ConfigureAwait(false);

                    // 다음 에이전트를 위한 입력 준비
                    PassOutputForward(stepResult, ref currentMessages, accumulatedMessages);

                    // 각 단계 완료 후 체크포인트 저장
                    await SaveCheckpointAsync(steps, currentMessages, cts.Token).ConfigureAwait(false);
                }

                stopwatch.Stop();

                var finalOutput = ExtractMessage(steps.LastOrDefault(s => s.IsSuccess)?.Response);
                if (finalOutput == null)
                {
                    await WriteFailedAsync(writer, NoSuccessfulOutput, steps, stopwatch.Elapsed, cancellationToken)
                        .ConfigureAwait(false);
                    return;
                }

                // 완료 시 체크포인트 삭제
                await DeleteCheckpointAsync(cancellationToken).ConfigureAwait(false);

                await writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.Completed,
                    Result = OrchestrationResult.Success(
                        finalOutput,
                        steps,
                        stopwatch.Elapsed,
                        AggregateTokenUsage(steps))
                }, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // 전체 오케스트레이션 타임아웃
                stopwatch.Stop();
                await WriteFailedAsync(writer, OrchestrationTimedOut(), steps, stopwatch.Elapsed, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            writer.Complete();
        }
    }

    private const string NoSuccessfulOutput = "No successful agent output";

    private static string ApprovalDenied(IAgent agent) => $"Approval denied for agent '{agent.Name}'";

    private static string FailureOf(AgentStepResult step) => step.Error ?? $"Agent '{step.AgentName}' failed";

    private string OrchestrationTimedOut() => $"Orchestration timed out after {Options.Timeout.TotalSeconds}s";

    private void PassOutputForward(AgentStepResult step, ref List<Message> currentMessages, List<Message> accumulatedMessages)
    {
        var outputMessage = ExtractMessage(step.Response);
        if (outputMessage == null || !Options.PassOutputAsInput)
        {
            return;
        }

        if (Options.AccumulateHistory)
        {
            accumulatedMessages.Add(outputMessage);
        }
        else
        {
            currentMessages = [outputMessage];
        }
    }
}
