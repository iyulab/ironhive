namespace IronHive.Abstractions.Message.Content;

/// <summary>
/// 파일의 콘텐츠 블록을 나타냅니다
/// </summary>
public class FileMessageContent : MessageContent
{
    /// <summary>
    /// 파일의 MIME 타입을 나타냅니다
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 데이터의 형식을 나타냅니다
    /// </summary>
    public DataFormat? DataFormat { get; set; }

    /// <summary>
    /// 데이터의 타입에 따른 값을 나타냅니다
    /// </summary>
    public string? Data { get; set; }
}

/// <summary>
/// 파일 콘텐츠의 데이터 형식을 나타냅니다
/// </summary>
public enum DataFormat
{
    /// <summary>
    /// 일반 텍스트입니다
    /// </summary>
    Text,

    /// <summary>
    /// Base64 인코딩된 데이터입니다
    /// </summary>
    Base64,

    /// <summary>
    /// URL 링크입니다
    /// </summary>
    Url,
}
