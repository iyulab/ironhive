using IronHive.Abstractions.Message.Content;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Message;

/// <summary>
/// 콘텐츠 블록의 기본 객체입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMessageContent), "text")]
[JsonDerivedType(typeof(FileMessageContent), "file")]
[JsonDerivedType(typeof(ToolMessageContent), "tool")]
[JsonDerivedType(typeof(ThinkingMessageContent), "thinking")]
public abstract class MessageContent
{
    /// <summary>
    /// delta 객체의 타입에 맞춰서 현재 객체에 반영됩니다.
    /// </summary>
    public void Merge(MessageDeltaContent delta)
    {
        switch ((this, delta))
        {
            case (TextMessageContent textContent, TextDeltaContent textDelta):
                textContent.Value += textDelta.Value;
                break;

            case (ToolMessageContent toolContent, ToolDeltaContent toolDelta):
                toolContent.Input += toolDelta.Input;
                break;

            case (ThinkingMessageContent thinkingContent, ThinkingDeltaContent thinkingDelta):
                thinkingContent.Value += thinkingDelta.Data;
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported delta type: content={GetType().Name}, delta={delta.GetType().Name}");
        }
    }

    /// <summary>
    /// 업데이트 객체의 타입에 맞춰서 현재 객체에 반영됩니다.
    /// </summary>
    public void Update(MessageUpdatedContent updated)
    {
        switch ((this, updated))
        {
            case (ThinkingMessageContent thinkingContent, ThinkingUpdatedContent thinkingUpdated):
                thinkingContent.Id = thinkingUpdated.Id;
                break;

            case (ToolMessageContent toolContent, ToolUpdatedContent toolUpdated):
                toolContent.Output = toolUpdated.Output;
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported update type: content={GetType().Name}, update={updated.GetType().Name}");
        }
    }
}
