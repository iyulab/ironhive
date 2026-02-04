using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 오케스트레이션 상태를 저장하고 복원하기 위한 체크포인트 저장소입니다.
/// </summary>
public interface ICheckpointStore
{
    /// <summary>
    /// 체크포인트를 저장합니다.
    /// </summary>
    Task SaveAsync(string orchestrationId, OrchestrationCheckpoint checkpoint, CancellationToken ct = default);

    /// <summary>
    /// 체크포인트를 로드합니다.
    /// </summary>
    Task<OrchestrationCheckpoint?> LoadAsync(string orchestrationId, CancellationToken ct = default);

    /// <summary>
    /// 체크포인트를 삭제합니다.
    /// </summary>
    Task DeleteAsync(string orchestrationId, CancellationToken ct = default);
}

/// <summary>
/// 오케스트레이션 실행 중간 상태를 나타내는 체크포인트입니다.
/// </summary>
public class OrchestrationCheckpoint
{
    /// <summary>
    /// 오케스트레이션 ID
    /// </summary>
    public required string OrchestrationId { get; init; }

    /// <summary>
    /// 오케스트레이터 이름
    /// </summary>
    public required string OrchestratorName { get; init; }

    /// <summary>
    /// 완료된 단계 수
    /// </summary>
    public int CompletedStepCount { get; init; }

    /// <summary>
    /// 완료된 단계 결과 목록
    /// </summary>
    public IReadOnlyList<AgentStepResult> CompletedSteps { get; init; } = [];

    /// <summary>
    /// 현재 메시지 상태
    /// </summary>
    public IReadOnlyList<Message> CurrentMessages { get; init; } = [];

    /// <summary>
    /// 체크포인트 생성 시각
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
