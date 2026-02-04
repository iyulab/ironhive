namespace IronHive.Abstractions.Resilience;

/// <summary>
/// AI 작업의 복원력(Resilience) 설정 옵션입니다.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// 복원력 기능 활성화 여부. 기본값: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 재시도 옵션
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Circuit Breaker 옵션
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// 타임아웃 옵션
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new();
}

/// <summary>
/// 재시도 옵션
/// </summary>
public class RetryOptions
{
    /// <summary>
    /// 재시도 활성화 여부. 기본값: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 최대 재시도 횟수. 기본값: 3
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// 초기 지연 시간. 기본값: 1초
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 최대 지연 시간. 기본값: 30초
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 지연 시간 증가 배율 (exponential backoff). 기본값: 2
    /// </summary>
    public double BackoffMultiplier { get; set; } = 2;

    /// <summary>
    /// Jitter 사용 여부. 기본값: true
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Circuit Breaker 옵션
/// </summary>
public class CircuitBreakerOptions
{
    /// <summary>
    /// Circuit Breaker 활성화 여부. 기본값: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 실패율 임계값 (0.0 ~ 1.0). 기본값: 0.5 (50%)
    /// </summary>
    public double FailureRatio { get; set; } = 0.5;

    /// <summary>
    /// 샘플링 기간. 기본값: 30초
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 최소 처리량 (샘플링 기간 내). 기본값: 10
    /// </summary>
    public int MinimumThroughput { get; set; } = 10;

    /// <summary>
    /// Open 상태 지속 시간. 기본값: 30초
    /// </summary>
    public TimeSpan BreakDuration { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// 타임아웃 옵션
/// </summary>
public class TimeoutOptions
{
    /// <summary>
    /// 타임아웃 활성화 여부. 기본값: true
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 전체 작업 타임아웃. 기본값: 120초
    /// </summary>
    public TimeSpan TotalTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// 스트리밍 시 청크 간 타임아웃. 기본값: 30초
    /// </summary>
    public TimeSpan ChunkTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
