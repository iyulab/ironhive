using System.Diagnostics;
using System.Runtime.CompilerServices;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 에이전트를 순차적으로 실행하는 오케스트레이터입니다.
/// 각 에이전트의 출력이 다음 에이전트의 입력으로 전달됩니다.
/// </summary>
public class SequentialOrchestrator : OrchestratorBase
{
    private new SequentialOrchestratorOptions Options => (SequentialOrchestratorOptions)base.Options;

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

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        try
        {
            foreach (var agent in Agents)
            {
                var input = Options.AccumulateHistory ? accumulatedMessages : currentMessages;
                var stepResult = await ExecuteAgentAsync(agent, input, cts.Token).ConfigureAwait(false);
                steps.Add(stepResult);

                if (!stepResult.IsSuccess)
                {
                    if (Options.StopOnAgentFailure)
                    {
                        stopwatch.Stop();
                        return OrchestrationResult.Failure(
                            stepResult.Error ?? $"Agent '{agent.Name}' failed",
                            steps,
                            stopwatch.Elapsed);
                    }
                    continue;
                }

                // 다음 에이전트를 위한 입력 준비
                var outputMessage = ExtractMessage(stepResult.Response);
                if (outputMessage != null && Options.PassOutputAsInput)
                {
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

            stopwatch.Stop();

            // 마지막 성공 응답을 최종 출력으로 사용
            var lastSuccessStep = steps.LastOrDefault(s => s.IsSuccess);
            var finalOutput = ExtractMessage(lastSuccessStep?.Response);

            if (finalOutput == null)
            {
                return OrchestrationResult.Failure(
                    "No successful agent output",
                    steps,
                    stopwatch.Elapsed);
            }

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
                $"Orchestration timed out after {Options.Timeout.TotalSeconds}s",
                steps,
                stopwatch.Elapsed);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 순차 오케스트레이터의 스트리밍은 각 에이전트 실행의 시작/완료 이벤트만 제공
        // 실제 스트리밍 청크는 복잡한 타입 변환이 필요하므로 단순화
        var result = await ExecuteAsync(messages, cancellationToken).ConfigureAwait(false);

        yield return new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started };

        foreach (var step in result.Steps)
        {
            yield return new OrchestrationStreamEvent
            {
                EventType = OrchestrationEventType.AgentStarted,
                AgentName = step.AgentName
            };

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
}
