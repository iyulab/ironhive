namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 핸드오프 대상 에이전트 정보
/// </summary>
public sealed class HandoffTarget
{
    /// <summary>
    /// 대상 에이전트 이름
    /// </summary>
    public required string AgentName { get; init; }

    /// <summary>
    /// 대상 에이전트에 대한 설명 (핸드오프 판단에 사용)
    /// </summary>
    public string? Description { get; init; }
}
