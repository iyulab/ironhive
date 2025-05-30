using System.Text.Json.Serialization;
using IronHive.Abstractions.Message.Roles;

namespace IronHive.Abstractions.Message;

/// <summary>
/// 메시지의 기본 객체입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
public abstract class Message
{
    /// <summary>
    /// 메시지의 유니크 식별자입니다.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
}
