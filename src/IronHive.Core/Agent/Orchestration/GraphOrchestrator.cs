using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;
using IronHive.Abstractions.Agent;
using IronHive.Abstractions.Agent.Orchestration;
using IronHive.Abstractions.Messages;
using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Messages.Roles;
using IronHive.Core.Telemetry;

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
public class GraphOrchestrator : OrchestratorBase
{
    private readonly Dictionary<string, AgentGraphNode> _nodes = new();
    private readonly List<AgentGraphEdge> _edges = [];
    private string? _startNodeId;
    private string? _outputNodeId;

    private new GraphOrchestratorOptions Options => (GraphOrchestratorOptions)base.Options;

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
        var nodeResults = new ConcurrentDictionary<string, AgentStepResult>();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(Options.Timeout);

        using var activity = IronHiveTelemetry.StartOrchestrationActivity(
            Name, "graph", Options.OrchestrationId);

        try
        {
            var inputMessages = messages.ToList();

            // 체크포인트에서 재개
            var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
            var completedSteps = checkpoint?.CompletedSteps.ToList() ?? [];
            var completedNodeIds = new HashSet<string>();

            // 체크포인트의 완료된 단계를 nodeResults에 복원
            foreach (var step in completedSteps)
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

            // 토폴로지 레벨별로 실행
            var levels = GetTopologicalLevels();

            foreach (var level in levels)
            {
                // 이미 완료된 노드 제외
                var nodesToExecute = level.Where(id => !completedNodeIds.Contains(id)).ToList();
                if (nodesToExecute.Count == 0) continue;

                // 레벨 내 노드들을 필터링 (엣지 조건, 승인 확인)
                var executableNodes = new List<(string NodeId, IEnumerable<Message> Input)>();

                foreach (var nodeId in nodesToExecute)
                {
                    var node = _nodes[nodeId];

                    // 인입 엣지 확인
                    var incomingEdges = _edges.Where(e => e.TargetId == nodeId).ToList();
                    IEnumerable<Message> nodeInput;

                    if (incomingEdges.Count == 0)
                    {
                        nodeInput = inputMessages;
                    }
                    else
                    {
                        var shouldExecute = true;
                        var collectedMessages = new List<Message>();

                        foreach (var edge in incomingEdges)
                        {
                            if (!nodeResults.TryGetValue(edge.SourceId, out var sourceResult))
                            {
                                shouldExecute = false;
                                break;
                            }

                            if (edge.Condition != null && !edge.Condition(sourceResult))
                            {
                                shouldExecute = false;
                                break;
                            }

                            var sourceMessage = ExtractMessage(sourceResult.Response);
                            if (sourceMessage != null)
                            {
                                collectedMessages.Add(sourceMessage);
                            }
                        }

                        if (!shouldExecute) continue;
                        nodeInput = collectedMessages.Count > 0 ? collectedMessages : inputMessages;
                    }

                    // 승인 체크 (순차)
                    var previousStep = steps.LastOrDefault();
                    if (!await CheckApprovalAsync(node.Agent, previousStep, cts.Token).ConfigureAwait(false))
                    {
                        await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                        stopwatch.Stop();
                        return OrchestrationResult.Failure(
                            $"Approval denied for agent '{node.Agent.Name}'",
                            steps,
                            stopwatch.Elapsed);
                    }

                    executableNodes.Add((nodeId, nodeInput));
                }

                if (executableNodes.Count == 0) continue;

                // 같은 레벨의 노드들을 병렬 실행
                if (executableNodes.Count == 1)
                {
                    // 단일 노드: 직접 실행
                    var (nodeId, nodeInput) = executableNodes[0];
                    var stepResult = await ExecuteAgentAsync(_nodes[nodeId].Agent, nodeInput, cts.Token).ConfigureAwait(false);
                    steps.Add(stepResult);
                    nodeResults[nodeId] = stepResult;

                    if (!stepResult.IsSuccess && Options.StopOnAgentFailure)
                    {
                        await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                        stopwatch.Stop();
                        return OrchestrationResult.Failure(
                            stepResult.Error ?? $"Agent '{_nodes[nodeId].Agent.Name}' failed",
                            steps,
                            stopwatch.Elapsed);
                    }
                }
                else
                {
                    // 복수 노드: 병렬 실행
                    var tasks = executableNodes.Select(async n =>
                    {
                        var result = await ExecuteAgentAsync(_nodes[n.NodeId].Agent, n.Input, cts.Token).ConfigureAwait(false);
                        nodeResults[n.NodeId] = result;
                        return (n.NodeId, Result: result);
                    });

                    var parallelResults = await Task.WhenAll(tasks).ConfigureAwait(false);

                    foreach (var (nodeId, result) in parallelResults)
                    {
                        steps.Add(result);
                    }

                    if (Options.StopOnAgentFailure)
                    {
                        var failed = parallelResults.FirstOrDefault(r => !r.Result.IsSuccess);
                        if (failed.NodeId != null)
                        {
                            await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                            stopwatch.Stop();
                            return OrchestrationResult.Failure(
                                failed.Result.Error ?? $"Agent '{_nodes[failed.NodeId].Agent.Name}' failed",
                                steps,
                                stopwatch.Elapsed);
                        }
                    }
                }

                // 레벨 완료 후 체크포인트 저장
                await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
            }

            stopwatch.Stop();

            // 출력 노드의 결과를 최종 출력으로 사용
            Message? finalOutput = null;
            if (_outputNodeId != null && nodeResults.TryGetValue(_outputNodeId, out var outputResult))
            {
                finalOutput = ExtractMessage(outputResult.Response);
            }

            if (finalOutput == null)
            {
                var lastSuccess = steps.LastOrDefault(s => s.IsSuccess);
                finalOutput = ExtractMessage(lastSuccess?.Response);
            }

            if (finalOutput == null)
            {
                return OrchestrationResult.Failure("No successful agent output", steps, stopwatch.Elapsed);
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
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var steps = new List<AgentStepResult>();
            var nodeResults = new Dictionary<string, AgentStepResult>();
            var inputMessages = messages.ToList();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(Options.Timeout);

            // 체크포인트에서 재개
            var checkpoint = await LoadCheckpointAsync(cts.Token).ConfigureAwait(false);
            var completedSteps = checkpoint?.CompletedSteps.ToList() ?? [];
            var completedNodeIds = new HashSet<string>();

            foreach (var step in completedSteps)
            {
                steps.Add(step);
                var matchingNodeId = _nodes.FirstOrDefault(n => n.Value.Agent.Name == step.AgentName).Key;
                if (matchingNodeId != null)
                {
                    nodeResults[matchingNodeId] = step;
                    completedNodeIds.Add(matchingNodeId);
                }
            }

            await writer.WriteAsync(
                new OrchestrationStreamEvent { EventType = OrchestrationEventType.Started },
                cancellationToken).ConfigureAwait(false);

            // 토폴로지 레벨을 평탄화하여 순차 실행 순서 생성
            var executionOrder = GetTopologicalLevels().SelectMany(level => level).ToList();

            foreach (var nodeId in executionOrder)
            {
                // 체크포인트에서 이미 완료된 노드는 건너뛰기
                if (completedNodeIds.Contains(nodeId)) continue;

                var node = _nodes[nodeId];

                // 인입 엣지 확인
                var incomingEdges = _edges.Where(e => e.TargetId == nodeId).ToList();
                IEnumerable<Message> nodeInput;

                if (incomingEdges.Count == 0)
                {
                    nodeInput = inputMessages;
                }
                else
                {
                    var shouldExecute = true;
                    var collectedMessages = new List<Message>();

                    foreach (var edge in incomingEdges)
                    {
                        if (!nodeResults.TryGetValue(edge.SourceId, out var sourceResult))
                        {
                            shouldExecute = false;
                            break;
                        }

                        if (edge.Condition != null && !edge.Condition(sourceResult))
                        {
                            shouldExecute = false;
                            break;
                        }

                        var sourceMessage = ExtractMessage(sourceResult.Response);
                        if (sourceMessage != null)
                        {
                            collectedMessages.Add(sourceMessage);
                        }
                    }

                    if (!shouldExecute) continue;
                    nodeInput = collectedMessages.Count > 0 ? collectedMessages : inputMessages;
                }

                // 승인 체크
                var previousStep = steps.LastOrDefault();
                if (!await CheckApprovalAsync(node.Agent, previousStep, cts.Token).ConfigureAwait(false))
                {
                    await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.Failed,
                        Error = $"Approval denied for agent '{node.Agent.Name}'",
                        Result = OrchestrationResult.Failure(
                            $"Approval denied for agent '{node.Agent.Name}'",
                            steps,
                            stopwatch.Elapsed)
                    }, cancellationToken).ConfigureAwait(false);
                    return;
                }

                await writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.AgentStarted,
                    AgentName = node.Agent.Name
                }, cancellationToken).ConfigureAwait(false);

                var agentStopwatch = Stopwatch.StartNew();
                var textBuilder = new StringBuilder();
                StreamingMessageDoneResponse? doneResponse = null;

                try
                {
                    using var agentCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
                    agentCts.CancelAfter(Options.AgentTimeout);

                    await foreach (var chunk in node.Agent.InvokeStreamingAsync(nodeInput, agentCts.Token).ConfigureAwait(false))
                    {
                        await writer.WriteAsync(new OrchestrationStreamEvent
                        {
                            EventType = OrchestrationEventType.MessageDelta,
                            AgentName = node.Agent.Name,
                            StreamingResponse = chunk
                        }, cancellationToken).ConfigureAwait(false);

                        switch (chunk)
                        {
                            case StreamingContentDeltaResponse delta:
                                if (delta.Delta is TextDeltaContent textDelta)
                                {
                                    textBuilder.Append(textDelta.Value);
                                }
                                break;
                            case StreamingMessageDoneResponse done:
                                doneResponse = done;
                                break;
                        }
                    }

                    agentStopwatch.Stop();

                    var responseMessage = new AssistantMessage
                    {
                        Content = [new TextMessageContent { Value = textBuilder.ToString() }]
                    };

                    var response = new MessageResponse
                    {
                        Id = doneResponse?.Id ?? Guid.NewGuid().ToString(),
                        DoneReason = doneResponse?.DoneReason,
                        Message = responseMessage,
                        TokenUsage = doneResponse?.TokenUsage,
                        Timestamp = doneResponse?.Timestamp ?? DateTime.UtcNow
                    };

                    var stepResult = new AgentStepResult
                    {
                        AgentName = node.Agent.Name,
                        Input = nodeInput.ToList(),
                        Response = response,
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = true
                    };
                    steps.Add(stepResult);
                    nodeResults[nodeId] = stepResult;

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentCompleted,
                        AgentName = node.Agent.Name,
                        CompletedResponse = response
                    }, cancellationToken).ConfigureAwait(false);

                    // 노드 완료 후 체크포인트 저장
                    await SaveCheckpointAsync(steps, inputMessages, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    agentStopwatch.Stop();
                    var errorMessage = $"Agent '{node.Agent.Name}' timed out";

                    var failResult = new AgentStepResult
                    {
                        AgentName = node.Agent.Name,
                        Input = nodeInput.ToList(),
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = false,
                        Error = errorMessage
                    };
                    steps.Add(failResult);
                    nodeResults[nodeId] = failResult;

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentFailed,
                        AgentName = node.Agent.Name,
                        Error = errorMessage
                    }, cancellationToken).ConfigureAwait(false);

                    if (Options.StopOnAgentFailure) break;
                }
                catch (Exception ex)
                {
                    agentStopwatch.Stop();
                    var errorMessage = $"Agent '{node.Agent.Name}' failed: {ex.Message}";

                    var failResult = new AgentStepResult
                    {
                        AgentName = node.Agent.Name,
                        Input = nodeInput.ToList(),
                        Duration = agentStopwatch.Elapsed,
                        IsSuccess = false,
                        Error = errorMessage
                    };
                    steps.Add(failResult);
                    nodeResults[nodeId] = failResult;

                    await writer.WriteAsync(new OrchestrationStreamEvent
                    {
                        EventType = OrchestrationEventType.AgentFailed,
                        AgentName = node.Agent.Name,
                        Error = errorMessage
                    }, cancellationToken).ConfigureAwait(false);

                    if (Options.StopOnAgentFailure) break;
                }
            }

            stopwatch.Stop();

            // 최종 결과 결정
            Message? finalOutput = null;
            if (_outputNodeId != null && nodeResults.TryGetValue(_outputNodeId, out var outputResult) && outputResult.IsSuccess)
            {
                finalOutput = ExtractMessage(outputResult.Response);
            }

            finalOutput ??= ExtractMessage(steps.LastOrDefault(s => s.IsSuccess)?.Response);

            if (finalOutput == null)
            {
                await writer.WriteAsync(new OrchestrationStreamEvent
                {
                    EventType = OrchestrationEventType.Failed,
                    Error = "No successful agent output",
                    Result = OrchestrationResult.Failure("No successful agent output", steps, stopwatch.Elapsed)
                }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
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
        }
        finally
        {
            writer.Complete();
        }
    }

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
