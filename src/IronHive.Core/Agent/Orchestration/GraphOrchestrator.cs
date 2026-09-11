using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Core.Utilities;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 그래프 기반 오케스트레이터 옵션
/// </summary>
public class GraphOrchestratorOptions : OrchestratorOptions
{
}

/// <summary>
/// DAG(Directed Acyclic Graph) 기반으로 에이전트를 실행하는 오케스트레이터입니다.
/// 엣지 조건에 따라 분기(Fan-Out)하고, 인입 엣지가 모두 완료된 노드만 실행(Fan-In)합니다.
/// </summary>
/// <remarks>
/// <see cref="ExecuteAsync"/>와 <see cref="ExecuteStreamingAsync"/>는 같은 규칙을 공유한다 — 체크포인트 복원,
/// 레벨 계획(엣지 조건과 승인), 레벨 기록(실패 판정), 최종 출력 결정. 다른 것은 한 레벨의 노드를 버퍼드는
/// 병렬로, 스트리밍은 델타를 섞지 않기 위해 순서대로 실행한다는 것뿐이다. 결과는 같다.
/// </remarks>
public class GraphOrchestrator : OrchestratorBase
{
    private readonly Dictionary<string, AgentGraphNode> _nodes = new();
    private readonly List<AgentGraphEdge> _edges = [];
    private string? _startNodeId;
    private string? _outputNodeId;

    private new GraphOrchestratorOptions Options => (GraphOrchestratorOptions)base.Options;

    /// <inheritdoc />
    public override bool SupportsRealTimeStreaming => true;

    internal GraphOrchestrator(
        GraphOrchestratorOptions options,
        IReadOnlyDictionary<string, AgentGraphNode> nodes,
        IReadOnlyList<AgentGraphEdge> edges,
        string startNodeId,
        string outputNodeId)
        : base(options)
    {
        foreach (var kvp in nodes)
        {
            _nodes[kvp.Key] = kvp.Value;
            AddAgent(kvp.Value.Agent);
        }
        _edges.AddRange(edges);
        _startNodeId = startNodeId;
        _outputNodeId = outputNodeId;
    }

    /// <inheritdoc />
    public override async Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default)
    {
        if (_nodes.Count == 0)
        {
            return OrchestrationResult.Failure("No nodes in the graph.");
        }

        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();
        var nodeResults = new Dictionary<string, AgentStepResult>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        using var activity = HiveTelemetry.StartOrchestrationActivity(
            Name, "graph", Options.OrchestrationId);

        try
        {
            var inputMessages = messages.ToList();
            var completedNodeIds = await RestoreCheckpointAsync(steps, nodeResults, cts.Token).ConfigureAwait(false);

            foreach (var level in GetTopologicalLevels())
            {
                var plan = await PlanLevelAsync(level, completedNodeIds, nodeResults, inputMessages, steps, cts.Token)
                    .ConfigureAwait(false);

                if (plan.DeniedAgent is { } denied)
                {
                    await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(ApprovalDenied(denied), steps, stopwatch.Elapsed);
                }

                if (plan.Nodes.Count == 0) continue;

                // 같은 레벨의 노드는 서로 독립이므로 병렬 실행
                var levelResults = await Task.WhenAll(plan.Nodes.Select(async n =>
                        (n.NodeId, Result: await ExecuteAgentAsync(_nodes[n.NodeId].Agent, n.Input, cts.Token).ConfigureAwait(false))))
                    .ConfigureAwait(false);

                if (RecordLevel(levelResults, steps, nodeResults) is { } failure)
                {
                    await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                    stopwatch.Stop();
                    return OrchestrationResult.Failure(failure, steps, stopwatch.Elapsed);
                }

                // 레벨 완료 후 체크포인트 저장
                await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
            }

            stopwatch.Stop();

            var finalOutput = ResolveFinalOutput(nodeResults, steps);
            if (finalOutput == null)
            {
                return OrchestrationResult.Failure(NoSuccessfulOutput, steps, stopwatch.Elapsed);
            }

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
            return OrchestrationResult.Failure(OrchestrationTimedOut(), steps, stopwatch.Elapsed);
        }
    }

    /// <inheritdoc />
    public override async IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_nodes.Count == 0)
        {
            yield return new OrchestrationStreamEvent
            {
                EventType = OrchestrationEventType.Failed,
                Error = "No nodes in the graph."
            };
            yield break;
        }

        var channel = Channel.CreateUnbounded<OrchestrationStreamEvent>();

        var producerTask = ProduceStreamingEventsAsync(channel.Writer, messages, cancellationToken);

        await foreach (var streamEvent in channel.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            yield return streamEvent;
        }

        await producerTask.ConfigureAwait(false);
    }

    private async Task ProduceStreamingEventsAsync(
        ChannelWriter<OrchestrationStreamEvent> writer,
        IEnumerable<Message> messages,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var steps = new List<AgentStepResult>();

        try
        {
            var nodeResults = new Dictionary<string, AgentStepResult>();
            var inputMessages = messages.ToList();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.Timeout);

            using var activity = HiveTelemetry.StartOrchestrationActivity(
                Name, "graph", Options.OrchestrationId);

            try
            {
                var completedNodeIds = await RestoreCheckpointAsync(steps, nodeResults, cts.Token).ConfigureAwait(false);

                await writer.WriteAsync(
                    new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started },
                    cancellationToken).ConfigureAwait(false);

                foreach (var level in GetTopologicalLevels())
                {
                    var plan = await PlanLevelAsync(level, completedNodeIds, nodeResults, inputMessages, steps, cts.Token)
                        .ConfigureAwait(false);

                    if (plan.DeniedAgent is { } denied)
                    {
                        await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                        stopwatch.Stop();
                        await WriteFailedAsync(writer, ApprovalDenied(denied), steps, stopwatch.Elapsed, cancellationToken)
                            .ConfigureAwait(false);
                        return;
                    }

                    // 델타가 섞이지 않도록 레벨 안의 노드는 순서대로 — 기록과 실패 판정은 레벨이 끝난 뒤, 버퍼드와 같이
                    var levelResults = new List<(string NodeId, AgentStepResult Result)>();
                    foreach (var (nodeId, nodeInput) in plan.Nodes)
                    {
                        var agent = _nodes[nodeId].Agent;

                        await writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.AgentStarted,
                            AgentName = agent.Name
                        }, cancellationToken).ConfigureAwait(false);

                        var step = await ExecuteAgentStreamingStepAsync(
                            agent,
                            nodeInput,
                            chunk => writer.WriteAsync(new OrchestrationStreamEvent
                            {
                                EventType = OrchestrationEventType.MessageDelta,
                                AgentName = agent.Name,
                                StreamingResponse = chunk
                            }, cancellationToken),
                            cts.Token).ConfigureAwait(false);

                        levelResults.Add((nodeId, step));

                        await writer.WriteAsync(step.IsSuccess
                            ? new OrchestrationStreamEvent
                            {
                                EventType = OrchestrationEventType.AgentCompleted,
                                AgentName = agent.Name,
                                CompletedResponse = step.Response
                            }
                            : new OrchestrationStreamEvent
                            {
                                EventType = OrchestrationEventType.AgentFailed,
                                AgentName = agent.Name,
                                Error = step.Error
                            }, cancellationToken).ConfigureAwait(false);
                    }

                    if (levelResults.Count == 0) continue;

                    if (RecordLevel(levelResults, steps, nodeResults) is { } failure)
                    {
                        await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                        stopwatch.Stop();
                        await WriteFailedAsync(writer, failure, steps, stopwatch.Elapsed, cancellationToken).ConfigureAwait(false);
                        return;
                    }

                    // 레벨 완료 후 체크포인트 저장
                    await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                }

                stopwatch.Stop();

                var finalOutput = ResolveFinalOutput(nodeResults, steps);
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

    private static string ApprovalDenied(string agentName) => $"Approval denied for agent '{agentName}'";

    private string OrchestrationTimedOut() => $"Orchestration timed out after {Options.Timeout.TotalSeconds}s";

    /// <summary>
    /// 체크포인트의 완료된 단계를 복원하고, 이미 완료된 노드 ID를 반환합니다.
    /// </summary>
    private async Task<HashSet<string>> RestoreCheckpointAsync(
        List<AgentStepResult> steps,
        Dictionary<string, AgentStepResult> nodeResults,
        CancellationToken cancellationToken)
    {
        var checkpoint = await LoadCheckpointAsync(cancellationToken).ConfigureAwait(false);
        var completedNodeIds = new HashSet<string>();

        foreach (var step in checkpoint?.CompletedSteps ?? [])
        {
            steps.Add(step);
            // AgentName → nodeId 매핑 (노드 ID와 에이전트 이름이 다를 수 있으므로 역매핑)
            var matchingNodeId = _nodes.FirstOrDefault(n => n.Value.Agent.Name == step.AgentName).Key;
            if (matchingNodeId != null)
            {
                nodeResults[matchingNodeId] = step;
                completedNodeIds.Add(matchingNodeId);
            }
        }

        return completedNodeIds;
    }

    /// <summary>
    /// 한 레벨에서 실행할 노드와 그 입력을 정합니다 — 이미 완료된 노드, 소스가 아직 실행되지 않은 노드,
    /// 조건이 거짓인 엣지를 가진 노드는 제외. 승인은 레벨의 노드를 실행하기 전에 모두 확인합니다.
    /// </summary>
    private async Task<LevelPlan> PlanLevelAsync(
        List<string> level,
        HashSet<string> completedNodeIds,
        Dictionary<string, AgentStepResult> nodeResults,
        List<Message> inputMessages,
        List<AgentStepResult> steps,
        CancellationToken cancellationToken)
    {
        var nodes = new List<(string NodeId, IEnumerable<Message> Input)>();

        foreach (var nodeId in level)
        {
            if (completedNodeIds.Contains(nodeId)) continue;

            var nodeInput = ResolveNodeInput(nodeId, nodeResults, inputMessages);
            if (nodeInput == null) continue;

            var agent = _nodes[nodeId].Agent;
            if (!await CheckApprovalAsync(agent, steps.LastOrDefault(), cancellationToken).ConfigureAwait(false))
            {
                return new LevelPlan(nodes, agent.Name);
            }

            nodes.Add((nodeId, nodeInput));
        }

        return new LevelPlan(nodes, DeniedAgent: null);
    }

    /// <summary>
    /// 노드의 입력 — 인입 엣지가 없으면 오케스트레이션 입력, 있으면 소스 노드들의 출력.
    /// 소스가 아직 실행되지 않았거나 엣지 조건이 거짓이면 null(실행하지 않음).
    /// </summary>
    private List<Message>? ResolveNodeInput(
        string nodeId,
        Dictionary<string, AgentStepResult> nodeResults,
        List<Message> inputMessages)
    {
        var incomingEdges = _edges.Where(e => e.TargetId == nodeId).ToList();
        if (incomingEdges.Count == 0)
        {
            return inputMessages;
        }

        var collectedMessages = new List<Message>();
        foreach (var edge in incomingEdges)
        {
            if (!nodeResults.TryGetValue(edge.SourceId, out var sourceResult))
            {
                return null;
            }

            if (edge.Condition != null && !edge.Condition(sourceResult))
            {
                return null;
            }

            var sourceMessage = ExtractMessage(sourceResult.Response);
            if (sourceMessage != null)
            {
                collectedMessages.Add(sourceMessage);
            }
        }

        return collectedMessages.Count > 0 ? collectedMessages : inputMessages;
    }

    /// <summary>
    /// 레벨의 결과를 계획 순서대로 기록하고, <see cref="OrchestratorOptions.StopOnAgentFailure"/>이면
    /// 첫 실패의 오류를 반환합니다(아니면 null).
    /// </summary>
    private string? RecordLevel(
        IEnumerable<(string NodeId, AgentStepResult Result)> levelResults,
        List<AgentStepResult> steps,
        Dictionary<string, AgentStepResult> nodeResults)
    {
        string? failure = null;

        foreach (var (nodeId, result) in levelResults)
        {
            steps.Add(result);
            nodeResults[nodeId] = result;

            if (!result.IsSuccess && Options.StopOnAgentFailure)
            {
                failure ??= result.Error ?? $"Agent '{_nodes[nodeId].Agent.Name}' failed";
            }
        }

        return failure;
    }

    /// <summary>
    /// 출력 노드가 성공했으면 그 결과, 아니면 마지막 성공 단계의 결과.
    /// </summary>
    private Message? ResolveFinalOutput(
        Dictionary<string, AgentStepResult> nodeResults,
        List<AgentStepResult> steps)
    {
        var fromOutputNode = _outputNodeId != null
            && nodeResults.TryGetValue(_outputNodeId, out var outputResult)
            && outputResult.IsSuccess
                ? ExtractMessage(outputResult.Response)
                : null;

        return fromOutputNode ?? ExtractMessage(steps.LastOrDefault(s => s.IsSuccess)?.Response);
    }

    private sealed record LevelPlan(List<(string NodeId, IEnumerable<Message> Input)> Nodes, string? DeniedAgent);

    /// <summary>
    /// Kahn 알고리즘으로 토폴로지 정렬 수행 (레벨별 그룹화)
    /// 같은 레벨의 노드들은 서로 독립적이므로 병렬 실행 가능
    /// </summary>
    private List<List<string>> GetTopologicalLevels()
    {
        var inDegree = new Dictionary<string, int>();
        var adjacency = new Dictionary<string, List<string>>();

        foreach (var nodeId in _nodes.Keys)
        {
            inDegree[nodeId] = 0;
            adjacency[nodeId] = [];
        }

        foreach (var edge in _edges)
        {
            adjacency[edge.SourceId].Add(edge.TargetId);
            inDegree[edge.TargetId]++;
        }

        var queue = new Queue<string>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                queue.Enqueue(kvp.Key);
            }
        }

        // 시작 노드를 우선 배치
        if (_startNodeId != null && inDegree.GetValueOrDefault(_startNodeId) == 0)
        {
            var reordered = new Queue<string>();
            reordered.Enqueue(_startNodeId);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (item != _startNodeId)
                    reordered.Enqueue(item);
            }
            queue = reordered;
        }

        var levels = new List<List<string>>();
        while (queue.Count > 0)
        {
            var currentLevel = new List<string>();
            var nextQueue = new Queue<string>();

            while (queue.Count > 0)
            {
                currentLevel.Add(queue.Dequeue());
            }

            foreach (var current in currentLevel)
            {
                foreach (var neighbor in adjacency[current])
                {
                    inDegree[neighbor]--;
                    if (inDegree[neighbor] == 0)
                    {
                        nextQueue.Enqueue(neighbor);
                    }
                }
            }

            levels.Add(currentLevel);
            queue = nextQueue;
        }

        return levels;
    }
}
