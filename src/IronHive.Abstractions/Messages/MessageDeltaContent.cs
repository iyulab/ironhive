using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextDeltaContent), "text")]
[JsonDerivedType(typeof(ToolDeltaContent), "tool")]
[JsonDerivedType(typeof(ThinkingDeltaContent), "thinking")]
public abstract class MessageDeltaContent
{ }

public class TextDeltaContent : MessageDeltaContent
{
    public required string Value { get; set; }
}

public class ToolDeltaContent : MessageDeltaContent
{
    public required string Input { get; set; }
}

public class ThinkingDeltaContent : MessageDeltaContent
{
    public required string Data { get; set; }
}
