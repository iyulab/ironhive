namespace Raggle.Abstractions.Messages;

/// <summary>
/// 메시지의 역할을 정의합니다.
/// </summary>
public enum MessageRole
{
    User,
    Assistant
}

/// <summary>
/// 채팅 메시지를 나타냅니다.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// 메시지의 역할을 나타냅니다.
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// 메시지의 내용을 담는 컬렉션입니다.
    /// </summary>
    public ICollection<IContentBlock> Contents { get; set; } = [];

    /// <summary>
    /// 메시지가 생성된 시간을 나타냅니다.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
