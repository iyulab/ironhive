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
{ }
