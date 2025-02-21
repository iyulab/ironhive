using System.Text.Json.Serialization;

namespace Raggle.Abstractions.ChatCompletion.Messages;

/// <summary>
/// 메시지의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(TextContent), "user")]
[JsonDerivedType(typeof(ImageContent), "assistant")]
public interface IMessage
{
    MessageContentCollection Content { get; set; }
    DateTime TimeStamp { get; set; }
}

/// <summary>
/// 메시지의 기본 클래스입니다.
/// </summary>
public abstract class MessageBase : IMessage
{
    public MessageContentCollection Content { get; set; } = new();
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 유저 메시지를 나타냅니다.
/// </summary>
public class UserMessage : MessageBase
{ }

/// <summary>
/// 어시스턴트 메시지를 나타냅니다.
/// </summary>
public class AssistantMessage : MessageBase
{
    public string? Name { get; set; }
}
