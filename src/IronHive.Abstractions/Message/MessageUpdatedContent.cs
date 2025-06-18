using IronHive.Abstractions.Message.Content;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Message;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ThinkingUpdatedContent), "thinking")]
[JsonDerivedType(typeof(ToolUpdatedContent), "tool")]
public abstract class MessageUpdatedContent
{ }

public class ThinkingUpdatedContent : MessageUpdatedContent
{
    public required string Id { get; set; }
}

public class ToolUpdatedContent : MessageUpdatedContent
{
    public required ToolContentStatus Status { get; set; }
    public string? Output { get; set; }
}
