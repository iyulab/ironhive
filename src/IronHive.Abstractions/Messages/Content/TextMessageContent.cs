namespace IronHive.Abstractions.Messages.Content;

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다
/// </summary>
public class TextMessageContent : MessageContent
{
    /// <summary>
    /// 텍스트 내용을 나타냅니다
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <inheritdoc />
    public override void Merge(MessageDeltaContent delta)
    {
        if (delta is TextDeltaContent textDelta)
            Value += textDelta.Value;
        else
            base.Merge(delta);
    }
}
