namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// GroupChat 종료 조건
/// </summary>
public interface ITerminationCondition
{
    /// <summary>
    /// 종료 조건이 충족되었는지 판단합니다.
    /// </summary>
    Task<bool> ShouldTerminateAsync(
        IReadOnlyList<AgentStepResult> steps,
        CancellationToken cancellationToken = default);
}
