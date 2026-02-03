using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 여러 에이전트를 조율하여 복잡한 작업을 수행하는 오케스트레이터입니다.
/// </summary>
public interface IAgentOrchestrator
{
    /// <summary>
    /// 오케스트레이터의 이름
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 등록된 에이전트 목록
    /// </summary>
    IReadOnlyList<IAgent> Agents { get; }

    /// <summary>
    /// 에이전트를 오케스트레이터에 추가합니다.
    /// </summary>
    /// <param name="agent">추가할 에이전트</param>
    void AddAgent(IAgent agent);

    /// <summary>
    /// 여러 에이전트를 오케스트레이터에 추가합니다.
    /// </summary>
    /// <param name="agents">추가할 에이전트들</param>
    void AddAgents(IEnumerable<IAgent> agents);

    /// <summary>
    /// 오케스트레이션을 실행합니다.
    /// </summary>
    /// <param name="messages">초기 입력 메시지</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>오케스트레이션 결과</returns>
    Task<OrchestrationResult> ExecuteAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 오케스트레이션을 스트리밍 방식으로 실행합니다.
    /// </summary>
    /// <param name="messages">초기 입력 메시지</param>
    /// <param name="cancellationToken">취소 토큰</param>
    /// <returns>각 에이전트의 스트리밍 응답</returns>
    IAsyncEnumerable<OrchestrationStreamEvent> ExecuteStreamingAsync(
        IEnumerable<Message> messages,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 오케스트레이션 스트리밍 이벤트
/// </summary>
public sealed class OrchestrationStreamEvent
{
    /// <summary>
    /// 이벤트 타입
    /// </summary>
    public required OrchestrationEventType EventType { get; init; }

    /// <summary>
    /// 관련 에이전트 이름
    /// </summary>
    public string? AgentName { get; init; }

    /// <summary>
    /// 스트리밍 응답 청크 (MessageDelta 이벤트 시)
    /// </summary>
    public StreamingMessageResponse? StreamingResponse { get; init; }

    /// <summary>
    /// 완료된 응답 (AgentCompleted 이벤트 시)
    /// </summary>
    public MessageResponse? CompletedResponse { get; init; }

    /// <summary>
    /// 최종 오케스트레이션 결과 (Completed 이벤트 시)
    /// </summary>
    public OrchestrationResult? Result { get; init; }

    /// <summary>
    /// 오류 메시지
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// 오케스트레이션 이벤트 타입
/// </summary>
public enum OrchestrationEventType
{
    /// <summary>
    /// 오케스트레이션 시작
    /// </summary>
    Started,

    /// <summary>
    /// 에이전트 실행 시작
    /// </summary>
    AgentStarted,

    /// <summary>
    /// 에이전트 메시지 델타 (스트리밍)
    /// </summary>
    MessageDelta,

    /// <summary>
    /// 에이전트 실행 완료
    /// </summary>
    AgentCompleted,

    /// <summary>
    /// 에이전트 실행 실패
    /// </summary>
    AgentFailed,

    /// <summary>
    /// 오케스트레이션 완료
    /// </summary>
    Completed,

    /// <summary>
    /// 오케스트레이션 실패
    /// </summary>
    Failed,

    /// <summary>
    /// 에이전트 실행 전 승인 대기 중
    /// </summary>
    ApprovalRequired,

    /// <summary>
    /// 승인됨
    /// </summary>
    ApprovalGranted,

    /// <summary>
    /// 승인 거부됨
    /// </summary>
    ApprovalDenied
}
