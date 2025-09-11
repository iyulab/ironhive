namespace IronHive.Abstractions.Agent;

/// <summary>
/// 에이전트의 다양한 속성을 정의하는 카드
/// </summary>
public class AgentCard
{
    /// <summary>
    /// 에이전트 이름
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// 에이전트 설명
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// 에이전트가 사용할 워크플로우 정의 (YAML 형식)
    /// </summary>
    public required string Workflow { get; init; }
    
    /// <summary>
    /// 에이전트가 사용할 초기 프롬프트 템플릿 (선택 사항)
    /// </summary>
    public string? PromptTemplate { get; init; }

    /// <summary>
    /// 에이전트가 사용할 컨텍스트 초기값 (선택 사항)
    /// </summary>
    public object? InitialContext { get; init; }
}