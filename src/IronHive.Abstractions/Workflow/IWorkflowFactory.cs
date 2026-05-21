namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 워크플로우 인스턴스를 생성하는 팩토리 인터페이스입니다.
/// DI에 등록된 <see cref="IWorkflowStep"/> 구현체를 사용하여 다양한 형식으로부터 워크플로우를 빌드합니다.
/// </summary>
public interface IWorkflowFactory
{
    /// <summary>
    /// 주어진 워크플로우 정의로부터 실행 가능한 워크플로우를 생성합니다.
    /// </summary>
    /// <typeparam name="TContext">워크플로우 실행 시 공유되는 컨텍스트 타입.</typeparam>
    /// <param name="definition">노드/스텝 구성이 포함된 워크플로우 정의.</param>
    /// <returns><see cref="IWorkflow{TContext}"/> 구현체.</returns>
    IWorkflow<TContext> CreateFrom<TContext>(WorkflowDefinition definition);

    /// <summary>
    /// YAML 문자열로부터 워크플로우를 생성합니다.
    /// </summary>
    /// <typeparam name="TContext">워크플로우 실행 컨텍스트 타입.</typeparam>
    /// <param name="yaml">YAML 형식의 워크플로우 정의 문자열.</param>
    /// <returns><see cref="IWorkflow{TContext}"/> 구현체.</returns>
    IWorkflow<TContext> CreateFromYaml<TContext>(string yaml);

    /// <summary>
    /// JSON 문자열로부터 워크플로우를 생성합니다.
    /// </summary>
    /// <typeparam name="TContext">워크플로우 실행 컨텍스트 타입.</typeparam>
    /// <param name="json">JSON 형식의 워크플로우 정의 문자열.</param>
    /// <returns><see cref="IWorkflow{TContext}"/> 구현체.</returns>
    IWorkflow<TContext> CreateFromJson<TContext>(string json);
}
