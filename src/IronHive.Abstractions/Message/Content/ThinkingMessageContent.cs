namespace IronHive.Abstractions.Message.Content;

/// <summary>
/// 추론 결과 콘텐츠 블록을 나타냅니다
/// </summary>
public class ThinkingMessageContent : MessageContent
{
    /// <summary>
    /// 콘텐츠 블록의 고유 ID입니다. 서비스 제공자의 ID를 사용합니다.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 추론의 형식 입니다.
    /// </summary>
    public ThinkingFormat? Format { get; set; }

    /// <summary>
    /// 추론 콘텐츠 블록의 내용입니다.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 추론 콘텐츠 블록의 형식을 나타내는 열거형입니다.
/// </summary>
public enum ThinkingFormat
{
    /// <summary>
    /// 전체 추론 맥락 
    /// </summary>
    Detailed,

    /// <summary>
    /// 보안 추론 컨텐츠
    /// </summary>
    Secure,

    /// <summary>
    /// 추론 요약 콘텐츠
    /// </summary>
    Summary
}
