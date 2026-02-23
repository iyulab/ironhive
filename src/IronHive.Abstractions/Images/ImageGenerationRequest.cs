namespace IronHive.Abstractions.Images;

/// <summary>
/// 이미지 생성 요청을 나타냅니다.
/// </summary>
public class ImageGenerationRequest
{
    /// <summary>
    /// 사용할 모델 ID
    /// </summary>
    public required string Model { get; set; }

    /// <summary>
    /// 이미지 생성 프롬프트
    /// </summary>
    public required string Prompt { get; set; }

    /// <summary>
    /// 생성할 이미지 수
    /// </summary>
    public int? N { get; set; }

    /// <summary>
    /// 생성할 이미지의 크기, 각 모델에서 지원하는 크기 옵션이 다를 수 있습니다.
    /// </summary>
    public GeneratedImageSize? Size { get; set; }
}
