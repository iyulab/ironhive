namespace IronHive.Abstractions.Streaming;

/// <summary>
/// 스트림 상태 관리 옵션
/// </summary>
public class StreamStateOptions
{
    /// <summary>
    /// 스트림 상태 만료 시간. 기본값: 30분
    /// </summary>
    public TimeSpan ExpirationTime { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// 자동 정리 활성화 여부. 기본값: true
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = true;

    /// <summary>
    /// 자동 정리 간격. 기본값: 5분
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 최대 청크 저장 수. 기본값: 1000
    /// </summary>
    public int MaxChunksPerStream { get; set; } = 1000;

    /// <summary>
    /// 재개 시도 최대 횟수. 기본값: 3
    /// </summary>
    public int MaxResumeAttempts { get; set; } = 3;

    /// <summary>
    /// 연결 끊김 후 재개 가능 시간. 기본값: 5분
    /// </summary>
    public TimeSpan ResumeWindow { get; set; } = TimeSpan.FromMinutes(5);
}
