namespace IronHive.Abstractions.Videos;

/// <summary>
/// 비디오 생성 서비스를 제공하는 애플리케이션 인터페이스입니다.
/// </summary>
public interface IVideoService
{
    /// <summary>
    /// 비디오를 생성합니다.
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoAsync(
        string provider,
        VideoGenerationRequest request,
        IProgress<VideoGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
