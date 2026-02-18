using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 에이전트를 병렬로 실행하는 오케스트레이터입니다.
/// 모든 에이전트가 동일한 입력을 받고 동시에 실행됩니다.
/// </summary>
public class ParallelOrchestrator : OrchestratorBase
{
    private new ParallelOrchestratorOptions Options => (ParallelOrchestratorOptions)base.Options;

    public ParallelOrchestrator(ParallelOrchestratorOptions? options = null)
        : base(options ?? new ParallelOrchestratorOptions())
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
        var inputMessages = messages.ToList();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        try
        {
            var tasks = Options.MaxConcurrency.HasValue
                ? ExecuteWithConcurrencyLimit(inputMessages, cts.Token)
                : ExecuteAllParallel(inputMessages, cts.Token);

            var steps = await tasks.ConfigureAwait(false);
            stopwatch.Stop();

            return ProcessResults(steps, stopwatch.Elapsed);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"Orchestration timed out after {Options.Timeout.TotalSeconds}s",
                duration: stopwatch.Elapsed);
        }
    }

    private async Task<List<AgentStepResult>> ExecuteAllParallel(
        List<Message> messages,
        CancellationToken cancellationToken)
    {
        var tasks = Agents.Select(agent => ExecuteAgentAsync(agent, messages, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToList();
    }

    private async Task<List<AgentStepResult>> ExecuteWithConcurrencyLimit(
        List<Message> messages,
        CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(Options.MaxConcurrency!.Value);

        var tasks = Agents.Select(async agent =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await ExecuteAgentAsync(agent, messages, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var completedResults = await Task.WhenAll(tasks).ConfigureAwait(false);
        return completedResults.ToList();
    }

    private OrchestrationResult ProcessResults(List<AgentStepResult> steps, TimeSpan duration)
    {
        var successfulSteps = steps.Where(s => s.IsSuccess).ToList();

        if (successfulSteps.Count == 0)
        {
            var errors = string.Join("; ", steps.Select(s => s.Error ?? "Unknown error"));
            return OrchestrationResult.Failure($"All agents failed: {errors}", steps, duration);
        }

        Message finalOutput;

        switch (Options.ResultAggregation)
        {
            case ParallelResultAggregation.FirstSuccess:
                finalOutput = ExtractMessage(successfulSteps.First().Response)!;
                break;

            case ParallelResultAggregation.Fastest:
                var fastest = successfulSteps.OrderBy(s => s.Duration).First();
                finalOutput = ExtractMessage(fastest.Response)!;
                break;

            case ParallelResultAggregation.Merge:
                finalOutput = MergeResponses(successfulSteps);
                break;

            case ParallelResultAggregation.All:
            default:
                // 마지막 성공 응답 사용
                finalOutput = ExtractMessage(successfulSteps.Last().Response)!;
                break;
        }

        return OrchestrationResult.Success(
            finalOutput,
            steps,
            duration,
            AggregateTokenUsage(steps));
    }

    private static AssistantMessage MergeResponses(List<AgentStepResult> steps)
    {
        var sb = new StringBuilder();

        foreach (var step in steps)
        {
            var msg = ExtractMessage(step.Response);
            if (msg != null)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"[{step.AgentName}]:");
                foreach (var content in GetMessageContent(msg))
                {
                    if (content is TextMessageContent textContent)
                    {
                        sb.AppendLine(textContent.Value);
                    }
                }
                sb.AppendLine();
            }
        }

        return new AssistantMessage
        {
            Content = [new TextMessageContent { Value = sb.ToString().Trim() }]
        };
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // 병렬 오케스트레이터의 스트리밍은 각 에이전트 실행의 시작/완료 이벤트만 제공
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
}
