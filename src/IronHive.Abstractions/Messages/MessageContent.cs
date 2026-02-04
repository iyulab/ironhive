using IronHive.Abstractions.Messages.Content;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 객체입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(ImageMessageContent), "image")]
[JsonDerivedType(typeof(DocumentMessageContent), "document")]
[JsonDerivedType(typeof(ToolMessageContent), "tool")]
[JsonDerivedType(typeof(ThinkingMessageContent), "thinking")]
public abstract class MessageContent
{
    /// <summary>
    /// delta 객체의 타입에 맞춰서 현재 객체에 반영됩니다.
    /// 파생 클래스에서 재정의하여 해당 델타 타입을 처리할 수 있습니다.
    /// </summary>
    public virtual void Merge(MessageDeltaContent delta)
    {
        throw new InvalidOperationException(
            $"Unsupported delta type: content={GetType().Name}, delta={delta.GetType().Name}");
    }

    /// <summary>
    /// 업데이트 객체의 타입에 맞춰서 현재 객체에 반영됩니다.
    /// 파생 클래스에서 재정의하여 해당 업데이트 타입을 처리할 수 있습니다.
    /// </summary>
    public virtual void Update(MessageUpdatedContent updated)
    {
        throw new InvalidOperationException(
            $"Unsupported update type: content={GetType().Name}, update={updated.GetType().Name}");
    }
}
