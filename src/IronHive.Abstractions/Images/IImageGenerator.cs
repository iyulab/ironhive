using IronHive.Abstractions.Registries;

namespace IronHive.Abstractions.Images;

/// <summary>
/// 이미지 생성 기능을 제공하는 프로바이더 인터페이스입니다.
/// <para>
/// 이 인터페이스는 <b>프로바이더 구현자</b>가 사용합니다.
/// OpenAI, Stability AI 등 특정 이미지 생성 제공자에 대한 저수준 어댑터 역할을 합니다.
/// </para>
/// <para>
/// 애플리케이션 코드에서는 <see cref="IImageService"/>를 사용하세요.
/// </para>
/// </summary>
public interface IImageGenerator : IProviderItem
{
    /// <summary>
    /// 텍스트 프롬프트로부터 이미지를 생성합니다.
    /// </summary>
    Task<ImageGenerationResponse> GenerateImageAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 기존 이미지를 편집합니다. (예: 인페인팅, 아웃페인팅)
    /// </summary>
    Task<ImageGenerationResponse> EditImageAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default);
}
