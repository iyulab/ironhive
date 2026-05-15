using IronHive.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignatureUpdatedContent), "signature")]
[JsonDerivedType(typeof(ToolUpdatedContent), "tool")]
public abstract class MessageUpdatedContent
{ }

public class SignatureUpdatedContent : MessageUpdatedContent
{
    public required string Signature { get; set; }
}

public class ToolUpdatedContent : MessageUpdatedContent
{
    public ToolOutput? Output { get; set; }
}
