namespace IronHive.Abstractions.Messages;

/// <summary>
/// 도구 실행 동작 설정입니다.
/// MessageRequest.ToolOptions에 지정합니다.
/// </summary>
public class ToolOptions
{
    /// <summary>
    /// 병렬로 동시에 실행할 최대 도구 수입니다.
    /// </summary>
    public int MaxParallel { get; set; } = 3;

    /// <summary>
    /// 개별 도구 실행의 최대 허용 시간입니다.
    /// null이면 제한 없음입니다.
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}
