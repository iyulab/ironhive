using IronHive.Abstractions.Messages;

namespace IronHive.Abstractions.Messages.Roles;

/// <summary>
/// 유저 메시지를 나타내는 클래스입니다.
/// </summary>
public class UserMessage : Message
{
    /// <summary>
    /// "text", "image", "document" 등의 메시지 콘텐츠를 포함하는 컬렉션입니다.
    /// </summary>
    public ICollection<MessageContent> Content { get; set; } = [];

    /// <summary>
    /// 메시지가 생성된 시간입니다.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
