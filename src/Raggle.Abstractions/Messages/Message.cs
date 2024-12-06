namespace Raggle.Abstractions.Messages;

/// <summary>
/// 채팅 메시지를 나타냅니다.
/// </summary>
public class Message
{
    /// <summary>
    /// 메시지의 역할을 나타냅니다.
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// 메시지를 보낸 사용자의 이름을 나타냅니다.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 메시지의 내용을 담는 컬렉션입니다.
    /// </summary>
    public MessageContentCollection Content { get; set; } = new MessageContentCollection();

    /// <summary>
    /// 메시지가 생성된 시간을 나타냅니다.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public Message Clone()
    {
        var clone = new Message();
        clone.Role = Role;
        foreach (var c in Content)
        {
            clone.Content.Add(c);
        }
        clone.TimeStamp = TimeStamp;
        return clone;
    }
}
