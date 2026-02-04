namespace IronHive.Abstractions.Messages.Content;

/// <summary>
/// 추론 결과 콘텐츠 블록을 나타냅니다
/// </summary>
public class ThinkingMessageContent : MessageContent
{
    /// <summary>
    /// 추론의 형식 입니다.
    /// </summary>
    public ThinkingFormat? Format { get; set; }

    /// <summary>
    /// 추론 메시지의 컨텍스트를 유지하기 위한 데이터입니다.
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// 추론 콘텐츠 블록의 내용입니다.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <inheritdoc />
    public override void Merge(MessageDeltaContent delta)
    {
        if (delta is ThinkingDeltaContent thinkingDelta)
            Value += thinkingDelta.Data;
        else
            base.Merge(delta);
    }

    /// <inheritdoc />
    public override void Update(MessageUpdatedContent updated)
    {
        if (updated is ThinkingUpdatedContent thinkingUpdated)
            Signature = thinkingUpdated.Signature;
        else
            base.Update(updated);
    }
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
