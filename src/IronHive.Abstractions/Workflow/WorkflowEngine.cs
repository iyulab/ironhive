namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 
/// </summary>
public sealed class WorkflowEngine<TContext> : IWorkflow<TContext>
{
    private readonly IReadOnlyDictionary<string, IWorkflowStep> _steps;

    public WorkflowEngine(IReadOnlyDictionary<string, IWorkflowStep> steps)
    {
        _steps = new Dictionary<string, IWorkflowStep>(steps, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public string? Id { get; init; }

    /// <inheritdoc />
    public Version? Version { get; init; }

    /// <summary>
    /// 실행 노드들 (순차 실행)
    /// </summary>
    public required IEnumerable<WorkflowNode> Nodes { get; init; }

    /// <inheritdoc />
    public event EventHandler<WorkflowEventArgs<TContext>>? Progressed;

    /// <inheritdoc />
    public Task RunAsync(TContext context, CancellationToken ct = default)
        => RunNodesAsync(Nodes, context, ct);

    /// <inheritdoc />
    public Task RunFromAsync(string nodeId, TContext context, CancellationToken ct = default)
    {
        var rest = SeekToNode(Nodes, nodeId)
                   ?? throw new InvalidOperationException($"nodeId '{nodeId}'를 찾을 수 없습니다.");
        return RunNodesAsync(rest, context, ct);
    }

    /// <summary>
    /// 노드를 순차적으로 실행합니다.
    /// </summary>
    private async Task RunNodesAsync(IEnumerable<WorkflowNode> nodes, TContext ctx, CancellationToken ct)
    {
        foreach (var node in nodes)
        {
            ct.ThrowIfCancellationRequested();
            var stepName = node switch
            {
                TaskNode t => t.Step,
                ConditionNode c => c.Step,
                ParallelNode p => null,
                EndNode => null,
                _ => null
            };

            OnStarted(ctx, node.Id, stepName);
            try
            {
                switch (node)
                {
                    case TaskNode t:
                        await ExecuteTaskNode(t, ctx, ct);
                        break;
                    case ConditionNode c:
                        var branch = await ExecuteConditionNode(c, ctx, ct);
                        await RunNodesAsync(branch, ctx, ct);
                        break;
                    case ParallelNode p:
                        await ExecuteParallelNode(p, ctx, ct);
                        break;
                    case EndNode:
                        return;
                }

                OnSucceeded(ctx, node.Id, stepName);
            }
            catch (Exception ex)
            {
                OnFailed(ctx, node.Id, stepName, ex);
                throw; // 실패 시 전체 중단
            }
        }
    }

    /// <summary>
    /// 작업 노드 실행을 수행합니다.
    /// </summary>
    private async Task ExecuteTaskNode(TaskNode node, TContext ctx, CancellationToken ct)
    {
        if (!_steps.TryGetValue(node.Step, out var step))
            throw new KeyNotFoundException($"'{node.Step}' 스텝이 등록되어 있지 않습니다.");

        var iface = step.GetType().GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IWorkflowTask<,>) &&
                                 i.GetGenericArguments()[0] == typeof(TContext))
            ?? throw new InvalidOperationException($"'{node.Step}'는 IWorkflowTask를 구현하지 않습니다.");

        var optionsType = iface.GetGenericArguments()[1];
        var options = node.With.ConvertTo(optionsType);

        var method = iface.GetMethod(nameof(IWorkflowTask<TContext, object?>.ExecuteAsync),
                                     new[] { typeof(TContext), optionsType, typeof(CancellationToken) })
                     ?? throw new MissingMethodException($"'{iface}'에서 ExecuteAsync 메서드를 찾을 수 없습니다.");

        var task = (Task<TaskStepResult>)method.Invoke(step, new object?[] { ctx!, options, ct })!;
        var result = await task.ConfigureAwait(false);

        if (result.IsError && result.Exception != null)
            throw result.Exception;
    }

    /// <summary>
    /// 조건 노드 실행을 수행합니다.
    /// </summary>
    private async Task<IEnumerable<WorkflowNode>> ExecuteConditionNode(ConditionNode node, TContext ctx, CancellationToken ct)
    {
        if (!_steps.TryGetValue(node.Step, out var step) || step is not IWorkflowCondition<TContext> cond)
            throw new KeyNotFoundException($"'{node.Step}' 스텝이 등록되어 있지 않습니다.");

        var res = await cond.EvaluateAsync(ctx, ct);
        if (!node.Branches.TryGetValue(res.Key, out var branch))
            throw new InvalidOperationException($"Condition '{node.Step}' 결과 '{res.Key}'에 해당하는 브랜치가 정의되지 않았습니다.");

        return branch;
    }

    /// <summary>
    /// 병렬 노드 실행을 수행합니다.
    /// </summary>
    private async Task ExecuteParallelNode(ParallelNode node, TContext ctx, CancellationToken ct)
    {
        switch (node.JoinMode)
        {
            case JoinMethod.WaitAll:
                await Task.WhenAll(node.Branches.Select(b => RunNodesAsync(b, ctx, ct)));
                break;
            case JoinMethod.WaitAny:
                await Task.WhenAny(node.Branches.Select(b => RunNodesAsync(b, ctx, ct)));
                break;
            default:
                throw new NotSupportedException($"{node.JoinMode}은 지원하지 않습니다.");
        }
    }

    /// <summary>
    /// 노드를 순차로 돌면서 ID가 일치하는 노드를 찾고, 그 노드부터 끝까지의 시퀀스를 반환합니다.
    /// </summary>
    private static IEnumerable<WorkflowNode>? SeekToNode(IEnumerable<WorkflowNode> nodes, string nodeId)
    {
        foreach (var n in nodes)
        {
            if ((n.Id ?? string.Empty) == nodeId)
                return nodes.SkipWhile(x => x != n);

            if (n is ConditionNode c)
            {
                foreach (var b in c.Branches.Values)
                {
                    var found = SeekToNode(b, nodeId);
                    if (found != null) return found;
                }
            }
            else if (n is ParallelNode p)
            {
                foreach (var b in p.Branches)
                {
                    var found = SeekToNode(b, nodeId);
                    if (found != null) return found;
                }
            }
        }
        return null;
    }

    // --- 이벤트 ---
    private void OnStarted(TContext context, string? nodeId, string? step) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Status = WorkflowProgressStatus.Started,
            Context = context,
            NodeId = nodeId,
            Step = step,
        });

    private void OnSucceeded(TContext context, string? nodeId, string? step) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Status = WorkflowProgressStatus.Succeeded,
            Context = context,
            NodeId = nodeId,
            Step = step
        });

    private void OnFailed(TContext context, string? nodeId, string? step, Exception? ex = null) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Status = WorkflowProgressStatus.Failed,
            Context = context,
            NodeId = nodeId,
            Step = step,
            Exception = ex
        });
}