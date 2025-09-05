using System.Text.Json.Serialization;
using IronHive.Abstractions.Messages.Roles;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 메시지의 기본 객체입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
public abstract class Message
{
    /// <summary>
    /// 메시지의 고유 식별자입니다.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 메시지가 생성된 시간입니다.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
