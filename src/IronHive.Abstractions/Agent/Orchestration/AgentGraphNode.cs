namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 그래프 오케스트레이터의 노드를 나타냅니다.
/// </summary>
public sealed class AgentGraphNode
{
    /// <summary>
    /// 노드 고유 ID
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// 이 노드에서 실행할 에이전트
    /// </summary>
    public required IAgent Agent { get; init; }
}

/// <summary>
/// 그래프 오케스트레이터의 엣지(간선)를 나타냅니다.
/// </summary>
public sealed class AgentGraphEdge
{
    /// <summary>
    /// 소스 노드 ID
    /// </summary>
    public required string SourceId { get; init; }

    /// <summary>
    /// 대상 노드 ID
    /// </summary>
    public required string TargetId { get; init; }

    /// <summary>
    /// 엣지 조건. null이면 항상 진행. true를 반환하면 해당 엣지를 따라 진행.
    /// </summary>
    public Func<AgentStepResult, bool>? Condition { get; init; }
}
