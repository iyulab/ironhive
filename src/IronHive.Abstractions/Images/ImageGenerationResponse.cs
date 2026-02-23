namespace IronHive.Abstractions.Images;

public class ImageGenerationResponse
{
    /// <summary>
    /// 생성된 이미지 목록
    /// </summary>
    public required ICollection<GeneratedImage> Images { get; set; }

    /// <summary>
    /// 생성된 이미지에 대한 설명 프롬프트
    /// </summary>
    public string? Prompt { get; set; }
}
