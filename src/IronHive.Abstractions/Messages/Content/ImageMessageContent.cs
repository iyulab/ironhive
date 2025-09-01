using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Messages.Content;

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다
/// </summary>
public class ImageMessageContent : MessageContent
{
    /// <summary>
    /// 이미지 형식을 나타냅니다.
    /// </summary>
    public required ImageFormat Format { get; set; }

    /// <summary>
    /// base64 인코딩된 이미지 데이터를 나타냅니다
    /// </summary>
    public required string Base64 { get; set; }
}

/// <summary>
/// 이미지 콘텐츠 블록의 형식을 나타내는 열거형입니다.
/// </summary>
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