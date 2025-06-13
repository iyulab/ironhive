namespace IronHive.Abstractions.Message.Content;

public enum ImageFormat
{
    /// <summary>
    /// JPEG 이미지
    /// </summary>
    Jpeg,

    /// <summary>
    /// PNG 이미지
    /// </summary>
    Png,

    /// <summary>
    /// GIF 이미지
    /// </summary>
    Gif,

    /// <summary>
    /// WebP 이미지
    /// </summary>
    Webp
}

public class ImageMessageContent : MessageContent
{
    /// <summary>
    /// 이미지 형식을 나타냅니다.
    /// </summary>
    public required ImageFormat Format { get; set; }

    /// <summary>
    /// 문자열 처리된 이미지 형식을 나타냅니다.
    /// </summary>
    public string FormatString
    {
        get {
            return Format switch
            {
                ImageFormat.Png => "image/png",
                ImageFormat.Jpeg => "image/jpeg",
                ImageFormat.Gif => "image/gif",
                ImageFormat.Webp => "image/webp",
                _ => throw new NotSupportedException($"not supported image format {Format}")
            };
        }
    }

    /// <summary>
    /// base64 인코딩된 이미지 데이터를 나타냅니다
    /// </summary>
    public required string Base64 { get; set; }
}
