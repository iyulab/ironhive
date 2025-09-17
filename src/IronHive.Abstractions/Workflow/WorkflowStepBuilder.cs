namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 워크플로우의 스텝 체인을 구성하는 빌더입니다.
/// - <see cref="Then{TStep}(string)"/> 및 오버로드로 태스크 스텝을 추가합니다.
/// - <see cref="When{TStep}(string, IReadOnlyDictionary{string, WorkflowStepBuilder{TContext}})"/> 로 분기 조건을 구성합니다.
/// - <see cref="Build"/> 로 <see cref="IWorkflow{TContext}"/> 인스턴스를 생성합니다.
/// </summary>
public class WorkflowStepBuilder<TContext>
{
    private readonly WorkflowDefinition _definition;
    private readonly IDictionary<string, IWorkflowStep> _steps;
    private readonly IServiceProvider? _services;

    public WorkflowStepBuilder(
        WorkflowDefinition? definition = null,
        IDictionary<string, IWorkflowStep>? steps = null,
        IServiceProvider? services = null)
    {
        _definition = definition ?? new WorkflowDefinition();
        _steps = steps ?? new Dictionary<string, IWorkflowStep>();
        _services = services;
    }

    /// <summary>
    /// 작업 스텝을 추가합니다.
    /// </summary>
    public WorkflowStepBuilder<TContext> Then<TStep>(string stepName)
        where TStep : class, IWorkflowTask<TContext>
    {
        AddStep<TStep>(stepName);
        _definition.Steps.Add(new TaskNode
        {
            Step = stepName
        });
        return this;
    }

    /// <summary>
    /// <typeparamref name="TOptions"/> 옵션을 사용하는 작업 스텝을 추가합니다.
    /// </summary>
    public WorkflowStepBuilder<TContext> Then<TStep, TOptions>(string stepName, TOptions options)
        where TStep : class, IWorkflowTask<TContext, TOptions>
    {
        AddStep<TStep>(stepName);
        _definition.Steps.Add(new TaskNode
        {
            Step = stepName,
            With = options
        });
        return this;
    }

    /// <summary>
    /// 조건 결과에 따른 분기 노드를 추가합니다.
    /// </summary>
    public WorkflowStepBuilder<TContext> Switch<TStep>(
        string stepName,
        IReadOnlyDictionary<string, Action<WorkflowStepBuilder<TContext>>> buildActions,
        Action<WorkflowStepBuilder<TContext>>? defaultBuildAction = null)
        where TStep : class, IWorkflowCondition<TContext>
    {
        AddStep<TStep>(stepName);
        
        var branches = buildActions.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var builder = CreateBranchBuilder();
                kvp.Value(builder);
                return builder._definition.Steps.AsEnumerable();
            });

        var defaultBranch = Enumerable.Empty<WorkflowNode>();
        if (defaultBuildAction != null)
        {
            var builder = CreateBranchBuilder();
            defaultBuildAction(builder);
            defaultBranch = builder._definition.Steps.AsEnumerable();
        }

        _definition.Steps.Add(new ConditionNode
        {
            Step = stepName,
            Branches = branches,
            DefaultBranch = defaultBranch
        });
        return this;
    }

    /// <summary>
    /// 병렬 실행 노드를 추가합니다.
    /// </summary>
    public WorkflowStepBuilder<TContext> Split(
        IEnumerable<Action<WorkflowStepBuilder<TContext>>> buildActions,
        JoinMode joinMode = JoinMode.WaitAll,
        ContextMode contextMode = ContextMode.Copied)
    {
        var branches = buildActions.Select(action =>
        {
            var builder = CreateBranchBuilder();
            action(builder);
            return builder._definition.Steps;
        }).ToList();

        _definition.Steps.Add(new ParallelNode
        {
            Join = joinMode,
            Context = contextMode,
            Branches = branches,
        });
        return this;
    }

    /// <summary>
    /// 구성된 워크플로를 빌드합니다.
    /// </summary>
    public IWorkflow<TContext> Build()
    {
        return new WorkflowEngine<TContext>(_steps.AsReadOnly())
        {
            Name = _definition.Name,
            Version = _definition.Version,
            Nodes = _definition.Steps
        };
    }

    /// <summary>
    /// 스텝을 찾거나 생성하여 추가합니다. 중복 추가는 무시합니다.
    /// </summary>
    private void AddStep<TStep>(string name) where TStep : class, IWorkflowStep
    {
        var step = _services.GetKeyedServiceOrCreate<TStep>(name);
        _steps.TryAdd(name, step);
    }

    /// <summary>
    /// 같은 스텝 서비스를 공유하는 브랜치 빌더를 생성합니다.
    /// </summary>
    private WorkflowStepBuilder<TContext> CreateBranchBuilder()
        => new(null, _steps, _services);
}
