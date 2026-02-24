namespace IronHive.Abstractions.Videos;

/// <summary>
/// 비디오 생성 진행 상태 콜백 데이터.
/// <see cref="IProgress{T}"/>를 통해 보고됩니다.
/// </summary>
public class VideoGenerationProgress
{
    /// <summary>
    /// 작업 완료 여부
    /// </summary>
    public bool Done { get; set; }

    /// <summary>
    /// Provider가 반환한 Operation ID (디버깅/로깅 용도)
    /// </summary>
    public string? OperationId { get; set; }

    /// <summary>
    /// 진행률 (0-100). Provider가 지원하는 경우에만 값이 있음.
    /// </summary>
    public int? Percent { get; set; }
}
