using IronHive.Abstractions.Messages.Content;
using IronHive.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

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
    public IToolOutput? Output { get; set; }
}
