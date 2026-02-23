namespace IronHive.Abstractions.Images;

/// <summary>
/// 생성된 이미지를 나타냅니다.
/// </summary>
public class GeneratedImage
{
    /// <summary>
    /// 이미지 바이너리 데이터
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// 생성된 이미지의 컨텐츠 타입 (예: "image/png", "image/jpeg")
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// 서비스 제공자가 수정한 프롬프트가 있는 경우, 수정된 프롬프트를 반환합니다.
    /// </summary>
    public string? RevisedPrompt { get; set; }

    /// <summary>
    /// Base64로 인코딩된 이미지 데이터를 포함하는 Data URI를 반환합니다.
    /// </summary>
    public string ToBase64()
    {
        if (string.IsNullOrEmpty(MimeType))
            throw new InvalidOperationException("MimeType이 설정되어야 합니다.");
        
        return $"data:{MimeType};base64,{Convert.ToBase64String(Data)}";
    }
}
