namespace IronHive.Abstractions.Message.Content;

/// <summary>
/// PDF, Word, PPT 등의 문서들의 내용을 담은 컨텐츠 입니다.
/// </summary>
public class DocumentMessageContent : MessageContent
{
    /// <summary>
    /// 파일의 MimeType을 나타냅니다.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 파일의 텍스트 내용을 나타냅니다.
    /// </summary>
    public required string Data { get; set; }
}
