using IronHive.Abstractions.Registries;

namespace IronHive.Abstractions.Videos;

/// <summary>
/// 비디오 생성 기능을 제공하는 프로바이더 인터페이스입니다.
/// </summary>
public interface IVideoGenerator : IProviderItem
{
    /// <summary>
    /// 비디오를 생성합니다. (텍스트, 이미지, 또는 둘 다 입력 가능)
    /// 내부적으로 Job 생성 → 상태 폴링 → 다운로드까지 처리하고 완성된 비디오를 반환합니다.
    /// progress 콜백으로 진행 상태를 보고합니다.
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoAsync(
        VideoGenerationRequest request,
        IProgress<VideoGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
