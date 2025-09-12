namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 인 코딩 빌더
/// </summary>
public class WorkflowBuilder<TContext>
{
    private readonly IServiceProvider? _services;
    private readonly WorkflowDefinition _definition = new();
    private readonly Dictionary<string, IWorkflowStep> _steps = new();

    public WorkflowBuilder(IServiceProvider? services = null)
    {
        _services = services;
    }

    public void WithId(string id) => _definition.Id = id;

    public void WithVersion(Version version) => _definition.Version = version;

    public WorkflowBuilder<TContext> Then<TStep>(string name)
        where TStep : class, IWorkflowTask<TContext>
    {
        Add<TStep>(name);
        _definition.Steps.Add(new TaskNode
        {
            Step = name
        });
        return this;
    }

    public WorkflowBuilder<TContext> Then<TStep, TOptions>(string name, TOptions options)
        where TStep : class, IWorkflowTask<TContext, TOptions>
    {
        Add<TStep>(name);
        _definition.Steps.Add(new TaskNode
        {
            Step = name,
            With = options
        });
        return this;
    }

    public WorkflowBuilder<TContext> When<TStep>(
        string name, 
        IReadOnlyDictionary<string, WorkflowBuilder<TContext>> branches)
        where TStep : class, IWorkflowCondition<TContext>
    {
        Add<TStep>(name);
        //_definition.Steps.Add(new ConditionNode
        //{
        //    Step = name,
        //    Branches = branches._definition.Steps
        //});
        return this;
    }

    public WorkflowBuilder<TContext> Parallel(IEnumerable<WorkflowBuilder<TContext>> branches)
    {
        //_definition.Steps.Add(new ParallelNode
        //{
        //    Branches = branches
        //});
        return this;
    }

    public IWorkflow<TContext> Build()
    {
        return new WorkflowEngine<TContext>(_steps)
        {
            Id = _definition.Id,
            Version = _definition.Version,
            Nodes = _definition.Steps
        };
    }

    /// <summary>
    /// 스텝을 찾거나 생성하여 추가합니다.
    /// </summary>
    private WorkflowBuilder<TContext> Add<TStep>(string name)
        where TStep : class, IWorkflowStep
    {
        var step = _services.GetKeyedServiceOrCreate<TStep>(name);
        _steps.Add(name, step);
        return this;
    }
}
