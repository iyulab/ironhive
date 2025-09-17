namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 워크플로우의 실행을 관리하는 제네릭 엔진 클래스입니다.
/// 정의된 노드(Nodes)에 따라 순차, 조건부, 병렬 실행 흐름을 제어합니다.
/// </summary>
public sealed class WorkflowEngine<TContext> : IWorkflow<TContext>
{
    private readonly IReadOnlyDictionary<string, IWorkflowStep> _steps;

    public WorkflowEngine(IReadOnlyDictionary<string, IWorkflowStep> steps)
    {
        _steps = steps;
    }

    /// <inheritdoc />
    public string? Name { get; init; }

    /// <inheritdoc />
    public Version? Version { get; init; }

    /// <summary>
    /// 워크플로우를 구성하는 노드들의 시퀀스입니다. 여기에 정의된 순서대로 워크플로우가 실행됩니다.
    /// </summary>
    public required IEnumerable<WorkflowNode> Nodes { get; init; }

    /// <inheritdoc />
    public event EventHandler<WorkflowEventArgs<TContext>>? Progressed;

    /// <inheritdoc />
    public async Task RunAsync(TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            OnStarted(context);
            await RunSequenceAsync(Nodes, context, cancellationToken);
            OnCompleted(context);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OnCancelled(context);
        }
        catch (Exception ex)
        {
            OnFailed(context, null, null, ex);
        }
    }

    /// <inheritdoc />
    public async Task RunFromAsync(string nodeId, TContext context, CancellationToken cancellationToken = default)
    {
        var nodes = SeekTo(Nodes, nodeId)
                   ?? throw new InvalidOperationException($"nodeId '{nodeId}'를 찾을 수 없습니다.");

        try
        {
            OnStarted(context);
            await RunSequenceAsync(nodes, context, cancellationToken);
            OnCompleted(context);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            OnCancelled(context);
        }
        catch (Exception ex)
        {
            OnFailed(context, nodeId, null, ex);
        }
    }

    /// <summary>
    /// 노드 트리 구조를 재귀적으로 탐색하여, 지정된 `nodeId`와 일치하는 노드를 찾고, 그 노드부터 시작하는 시퀀스를 반환합니다.
    /// </summary>
    private static IEnumerable<WorkflowNode>? SeekTo(IEnumerable<WorkflowNode> nodes, string nodeId)
    {
        foreach (var n in nodes)
        {
            if ((n.Id ?? string.Empty) == nodeId)
                return nodes.SkipWhile(x => x != n);

            if (n is ConditionNode c)
            {
                foreach (var b in c.Branches.Values)
                {
                    var branches = SeekTo(b, nodeId);
                    if (branches != null) return branches;
                }
            }

            if (n is ParallelNode p)
            {
                foreach (var b in p.Branches)
                {
                    var branches = SeekTo(b, nodeId);
                    if (branches != null) return branches;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 제공된 노드 시퀀스를 순차적으로 실행합니다.
    /// 재귀적으로 호출되어 조건 분기 등의 흐름을 처리합니다.
    /// </summary>
    private async Task RunSequenceAsync(IEnumerable<WorkflowNode> nodes, TContext ctx, CancellationToken ct)
    {
        foreach (var node in nodes)
        {
            ct.ThrowIfCancellationRequested();
            var stepName = node switch
            {
                TaskNode t => t.Step,
                ConditionNode c => c.Step,
                ParallelNode p => null,
                _ => null
            };

            OnProgressed(ctx, node.Id, stepName);
            switch (node)
            {
                case TaskNode t:
                    await ExecuteTaskAsync(t, ctx, ct);
                    break;
                case ConditionNode c:
                    var branch = await ExecuteConditionAsync(c, ctx, ct);
                    await RunSequenceAsync(branch, ctx, ct);
                    break;
                case ParallelNode p:
                    await ExecuteParallelAsync(p, ctx, ct);
                    break;
                default:
                    throw new NotSupportedException($"지원하지 않는 노드 타입: {node.GetType().FullName}");
            }
            OnProgressed(ctx, node.Id, stepName);
        }
    }

    /// <summary>
    /// 작업 노드 실행을 수행합니다.
    /// </summary>
    private async Task ExecuteTaskAsync(TaskNode node, TContext ctx, CancellationToken ct)
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
                                     [typeof(TContext), optionsType, typeof(CancellationToken)])
                     ?? throw new MissingMethodException($"'{iface}'에서 ExecuteAsync 메서드를 찾을 수 없습니다.");

        var task = (Task<TaskStepResult>)method.Invoke(step, [ctx, options, ct])!;
        var result = await task.ConfigureAwait(false);

        if (result.IsError)
            throw result.Exception ?? new InvalidOperationException(result.Message ?? "작업이 실패했습니다.");
    }

    /// <summary>
    /// ConditionNode를 실행하고, 평가 결과에 따라 다음으로 실행할 브랜치(노드 시퀀스)를 반환합니다.
    /// </summary>
    private async Task<IEnumerable<WorkflowNode>> ExecuteConditionAsync(ConditionNode node, TContext ctx, CancellationToken ct)
    {
        if (!_steps.TryGetValue(node.Step, out var step) || step is not IWorkflowCondition<TContext> cond)
            throw new KeyNotFoundException($"'{node.Step}' 스텝이 등록되어 있지 않습니다.");

        var res = await cond.EvaluateAsync(ctx, ct);

        if (node.Branches.TryGetValue(res.Key, out var branch))
            return branch;
        else if (node.DefaultBranch != null)
            return node.DefaultBranch;
        else
            throw new InvalidOperationException($"분기 키 '{res.Key}'에 해당하는 브랜치를 찾을 수 없습니다.");
    }

    /// <summary>
    /// ParallelNode를 실행합니다. 정의된 Join 방식에 따라 모든 브랜치 또는 일부 브랜치가 완료될 때까지 기다립니다.
    /// </summary>
    private async Task ExecuteParallelAsync(ParallelNode node, TContext ctx, CancellationToken ct)
    {
        if (node.Join == JoinMode.WaitAny)
        {
            await Task.WhenAny(node.Branches.Select(b =>
            {
                var bctx = node.Context == ContextMode.Copied ? ctx.Clone() : ctx;
                return RunSequenceAsync(b, bctx, ct);
            }));
        }
        else
        {
            await Task.WhenAll(node.Branches.Select(b =>
            {
                var bctx = node.Context == ContextMode.Copied ? ctx.Clone() : ctx;
                return RunSequenceAsync(b, bctx, ct);
            }));
        }
    }

    #region Event Methods

    private void OnStarted(TContext context) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Type = WorkflowProgressType.Started,
            Context = context
        });

    private void OnProgressed(TContext context, string? nodeId, string? step) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Type = WorkflowProgressType.Progressed,
            Context = context,
            NodeId = nodeId,
            StepName = step,
        });

    private void OnCompleted(TContext context) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Type = WorkflowProgressType.Completed,
            Context = context,
        });

    private void OnCancelled(TContext context) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Type = WorkflowProgressType.Cancelled,
            Context = context,
        });

    private void OnFailed(TContext context, string? nodeId, string? step, Exception? ex = null) =>
        Progressed?.Invoke(this, new WorkflowEventArgs<TContext>
        {
            Type = WorkflowProgressType.Failed,
            Context = context,
            NodeId = nodeId,
            StepName = step,
            Exception = ex
        });

    #endregion
}