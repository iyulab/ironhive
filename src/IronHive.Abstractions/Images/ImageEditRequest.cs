namespace IronHive.Abstractions.Images;

/// <summary>
/// 이미지 편집 요청을 나타냅니다 (인페인팅, 아웃페인팅).
/// </summary>
public class ImageEditRequest
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
    /// 편집할 원본 이미지 데이터
    /// </summary>
    public required IEnumerable<GeneratedImage> Images { get; set; }

    /// <summary>
    /// 편집 마스크 이미지 데이터 (선택 사항)
    /// <para>마스크는 원본 이미지와 동일한 크기여야 하며, 편집할 영역을 흰색(255)으로 표시합니다.</para>
    /// </summary>
    public GeneratedImage? Mask { get; set; }

    /// <summary>
    /// 생성할 이미지 수
    /// </summary>
    public int? N { get; set; }

    /// <summary>
    /// 생성할 이미지의 크기, 각 모델에서 지원하는 크기 옵션이 다를 수 있습니다.
    /// </summary>
    public GeneratedImageSize? Size { get; set; }
}
