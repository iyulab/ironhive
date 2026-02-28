using System.Diagnostics;
using System.Globalization;
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
            // 체크포인트에서 재개
            var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
            var completedSteps = checkpoint?.CompletedSteps.ToList() ?? [];
            var completedAgentNames = new HashSet<string>(completedSteps.Select(s => s.AgentName));

            // 이미 완료된 에이전트를 제외한 실행 대상
            var agentsToExecute = Agents.Where(a => !completedAgentNames.Contains(a.Name)).ToList();

            // 승인 체크 (병렬 실행 전에 순차 확인)
            var approvedAgents = new List<IAgent>();
            foreach (var agent in agentsToExecute)
            {
                if (!await CheckApprovalAsync(agent, completedSteps.LastOrDefault(), cts.Token).ConfigureAwait(false))
                {
                    await SaveCheckpointAsync(completedSteps, inputMessages, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(
                        $"Approval denied for agent '{agent.Name}'",
                        completedSteps,
                        stopwatch.Elapsed);
                }
                approvedAgents.Add(agent);
            }

            // 승인된 에이전트 병렬 실행
            List<AgentStepResult> newSteps;
            if (approvedAgents.Count > 0)
            {
                newSteps = Options.MaxConcurrency.HasValue
                    ? await ExecuteWithConcurrencyLimit(approvedAgents, inputMessages, cts.Token).ConfigureAwait(false)
                    : await ExecuteAllParallel(approvedAgents, inputMessages, cts.Token).ConfigureAwait(false);
            }
            else
            {
                newSteps = [];
            }

            // 이전 체크포인트 결과와 병합
            var allSteps = new List<AgentStepResult>(completedSteps);
            allSteps.AddRange(newSteps);

            stopwatch.Stop();

            var result = ProcessResults(allSteps, stopwatch.Elapsed);

            // 완료 시 체크포인트 삭제
            if (result.IsSuccess)
            {
                await DeleteCheckpointAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await SaveCheckpointAsync(allSteps, inputMessages, cts.Token).ConfigureAwait(false);
            }

            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"Orchestration timed out after {Options.Timeout.TotalSeconds}s",
                duration: stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return OrchestrationResult.Failure(
                $"Parallel orchestration failed: {ex.Message}",
                duration: stopwatch.Elapsed);
        }
    }

    private async Task<List<AgentStepResult>> ExecuteAllParallel(
        IReadOnlyList<IAgent> agents,
        List<Message> messages,
        CancellationToken cancellationToken)
    {
        var tasks = agents.Select(agent => ExecuteAgentAsync(agent, messages, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.ToList();
    }

    private async Task<List<AgentStepResult>> ExecuteWithConcurrencyLimit(
        IReadOnlyList<IAgent> agents,
        List<Message> messages,
        CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(Options.MaxConcurrency!.Value);

        var tasks = agents.Select(async agent =>
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
    public override IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
        => WrapAsStreamAsync(messages, cancellationToken);
}
