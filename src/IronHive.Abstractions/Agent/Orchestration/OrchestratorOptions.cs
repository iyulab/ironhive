using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Agent.Orchestration;

/// <summary>
/// 오케스트레이터 공통 옵션
/// </summary>
public class OrchestratorOptions
{
    /// <summary>
    /// 오케스트레이터 이름
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 전체 오케스트레이션 타임아웃. 기본값: 5분
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 개별 에이전트 타임아웃. 기본값: 2분
    /// </summary>
    public TimeSpan AgentTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 에이전트 실패 시 전체 오케스트레이션 중단 여부. 기본값: true
    /// </summary>
    public bool StopOnAgentFailure { get; set; } = true;

    /// <summary>
    /// 체크포인트 저장소. null이면 체크포인팅 비활성화.
    /// </summary>
    public ICheckpointStore? CheckpointStore { get; set; }

    /// <summary>
    /// 오케스트레이션 ID. 체크포인트 식별에 사용. null이면 자동 생성.
    /// </summary>
    public string? OrchestrationId { get; set; }

    /// <summary>
    /// 에이전트 실행 전 승인 핸들러.
    /// (agentName, previousStep) → true(진행) / false(중단).
    /// null이면 승인 없이 진행.
    /// </summary>
    public Func<string, AgentStepResult?, Task<bool>>? ApprovalHandler { get; set; }

    /// <summary>
    /// 승인이 필요한 에이전트 이름 집합. null이면 모든 에이전트에 ApprovalHandler 적용.
    /// </summary>
    public HashSet<string>? RequireApprovalForAgents { get; set; }

    /// <summary>
    /// 에이전트 실행 시 적용할 미들웨어 체인.
    /// null이거나 비어있으면 미들웨어 없이 에이전트 직접 실행.
    /// </summary>
    public IList<IAgentMiddleware>? AgentMiddlewares { get; set; }
}

/// <summary>
/// 순차 오케스트레이터 옵션
/// </summary>
public class SequentialOrchestratorOptions : OrchestratorOptions
{
    /// <summary>
    /// 이전 에이전트의 출력을 다음 에이전트의 입력으로 전달할지 여부. 기본값: true
    /// </summary>
    public bool PassOutputAsInput { get; set; } = true;

    /// <summary>
    /// 모든 메시지 히스토리를 누적하여 전달할지 여부. 기본값: false
    /// </summary>
    public bool AccumulateHistory { get; set; }
}

/// <summary>
/// 병렬 오케스트레이터 옵션
/// </summary>
public class ParallelOrchestratorOptions : OrchestratorOptions
{
    /// <summary>
    /// 최대 동시 실행 에이전트 수. 기본값: 무제한
    /// </summary>
    public int? MaxConcurrency { get; set; }

    /// <summary>
    /// 결과 집계 방식
    /// </summary>
    public ParallelResultAggregation ResultAggregation { get; set; } = ParallelResultAggregation.All;
}

/// <summary>
/// Hub-Spoke 오케스트레이터 옵션
/// </summary>
public class HubSpokeOrchestratorOptions : OrchestratorOptions
{
    /// <summary>
    /// 최대 라운드 수 (무한 루프 방지). 기본값: 10
    /// </summary>
    public int MaxRounds { get; set; } = 10;

    /// <summary>
    /// Spoke 에이전트 병렬 실행 여부. 기본값: false
    /// </summary>
    public bool ParallelSpokes { get; set; }

    /// <summary>
    /// 병렬 실행 시 최대 동시 Spoke 수
    /// </summary>
    public int? MaxConcurrentSpokes { get; set; }
}

/// <summary>
/// 핸드오프 오케스트레이터 옵션
/// </summary>
public class HandoffOrchestratorOptions : OrchestratorOptions
{
    /// <summary>
    /// 초기 에이전트 이름
    /// </summary>
    public required string InitialAgentName { get; set; }

    /// <summary>
    /// 최대 전환(handoff) 횟수. 기본값: 20
    /// </summary>
    public int MaxTransitions { get; set; } = 20;

    /// <summary>
    /// 핸드오프가 감지되지 않았을 때 호출되는 핸들러.
    /// null 반환 = 종료, Message 반환 = 해당 메시지로 현재 에이전트 계속 실행.
    /// null이면 핸드오프 없을 시 자동 종료.
    /// </summary>
    public Func<string, AgentStepResult, Task<Message?>>? NoHandoffHandler { get; set; }
}

/// <summary>
/// GroupChat 오케스트레이터 옵션
/// </summary>
public class GroupChatOrchestratorOptions : OrchestratorOptions
{
    /// <summary>
    /// 다음 발언자를 선택하는 전략
    /// </summary>
    public required ISpeakerSelector SpeakerSelector { get; set; }

    /// <summary>
    /// 종료 조건
    /// </summary>
    public required ITerminationCondition TerminationCondition { get; set; }

    /// <summary>
    /// 최대 라운드 수 (안전 한도). 기본값: 50
    /// </summary>
    public int MaxRounds { get; set; } = 50;
}

/// <summary>
/// 병렬 결과 집계 방식
/// </summary>
public enum ParallelResultAggregation
{
    /// <summary>
    /// 모든 결과를 개별적으로 반환
    /// </summary>
    All,

    /// <summary>
    /// 첫 번째 성공 결과만 반환
    /// </summary>
    FirstSuccess,

    /// <summary>
    /// 가장 빠른 결과만 반환
    /// </summary>
    Fastest,

    /// <summary>
    /// 모든 결과를 하나의 메시지로 병합
    /// </summary>
    Merge
}
