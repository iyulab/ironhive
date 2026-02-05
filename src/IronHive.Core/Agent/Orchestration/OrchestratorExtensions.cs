using IronHive.Abstractions.Agent.Orchestration;

namespace IronHive.Core.Agent.Orchestration;

/// <summary>
/// 오케스트레이터 관련 확장 메서드
/// </summary>
public static class OrchestratorExtensions
{
    /// <summary>
    /// 오케스트레이터를 IAgent로 래핑합니다.
    /// </summary>
    public static OrchestratorAgentAdapter AsAgent(
        this IAgentOrchestrator orchestrator,
        string? name = null,
        string? description = null)
    {
        return new OrchestratorAgentAdapter(orchestrator, name, description);
    }
}
