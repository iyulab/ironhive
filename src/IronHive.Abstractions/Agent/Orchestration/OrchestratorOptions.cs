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
    public bool AccumulateHistory { get; set; } = false;
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
    public bool ParallelSpokes { get; set; } = false;

    /// <summary>
    /// 병렬 실행 시 최대 동시 Spoke 수
    /// </summary>
    public int? MaxConcurrentSpokes { get; set; }
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
