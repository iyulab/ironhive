using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Message;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextDeltaContent), "text")]
[JsonDerivedType(typeof(ToolDeltaContent), "tool")]
[JsonDerivedType(typeof(ThinkingDeltaContent), "thinking")]
public abstract class MessageDeltaContent
{ }

public class TextDeltaContent : MessageDeltaContent
{
    public required string Text { get; set; }
}

public class ToolDeltaContent : MessageDeltaContent
{
    public required string Input { get; set; }
}

public class ThinkingDeltaContent : MessageDeltaContent
{
    public required string Data { get; set; }
}
