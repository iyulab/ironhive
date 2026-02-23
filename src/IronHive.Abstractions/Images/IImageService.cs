namespace IronHive.Abstractions.Images;

/// <summary>
/// 이미지 생성 서비스를 제공하는 애플리케이션 인터페이스입니다.
/// <para>
/// 이 인터페이스는 <b>애플리케이션 코드</b>가 사용합니다.
/// 여러 프로바이더를 통합하고 이미지 생성 작업을 관리하는 고수준 오케스트레이터입니다.
/// </para>
/// <para>
/// 프로바이더 구현자는 <see cref="IImageGenerator"/>를 구현하세요.
/// </para>
/// </summary>
public interface IImageService
{
    /// <summary>
    /// 지정된 프로바이더와 모델을 사용하여 이미지를 생성합니다.
    /// </summary>
    /// <param name="provider">사용할 프로바이더의 이름입니다.</param>
    /// <param name="request">이미지 생성 요청입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>생성된 이미지 응답을 반환합니다.</returns>
    Task<ImageGenerationResponse> GenerateImageAsync(
        string provider,
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 이미지를 편집합니다.
    /// </summary>
    /// <param name="provider">사용할 프로바이더의 이름입니다.</param>
    /// <param name="request">이미지 편집 요청입니다.</param>
    /// <param name="cancellationToken">취소 토큰입니다.</param>
    /// <returns>편집된 이미지 응답을 반환합니다.</returns>
    Task<ImageGenerationResponse> EditImageAsync(
        string provider,
        ImageEditRequest request,
        CancellationToken cancellationToken = default);
}
