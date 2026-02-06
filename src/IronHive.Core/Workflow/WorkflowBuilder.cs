namespace IronHive.Core.Workflow;

/// <summary>
/// 워크플로우 정의를 유창한(Fluent) 방식으로 구성하는 루트 빌더입니다.
/// - 이름/버전을 설정하고, <see cref="StartWith{TContext}"/>로 스텝 빌더 체인을 시작합니다.
/// </summary>
public class WorkflowBuilder
{
    private readonly IServiceProvider? _services;
    private readonly Abstractions.Workflow.WorkflowDefinition _definition = new();

    public WorkflowBuilder(IServiceProvider? services = null)
    {
        _services = services;
    }

    /// <summary>
    /// 워크플로우의 이름을 설정합니다.
    /// </summary>
    public WorkflowBuilder WithName(string name)
    {
        _definition.Name = name;
        return this;
    }

    /// <summary>
    /// 워크플로우의 버전을 설정합니다.
    /// </summary>
    public WorkflowBuilder WithVersion(Version version)
    {
        _definition.Version = version;
        return this;
    }

    /// <summary>
    /// 지정한 컨텍스트 타입으로 스텝 체인을 시작합니다.
    /// </summary>
    public WorkflowStepBuilder<TContext> StartWith<TContext>()
        => new(_definition, null, _services);
}
