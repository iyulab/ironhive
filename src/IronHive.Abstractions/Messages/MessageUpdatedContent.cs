using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SignatureUpdatedContent), "signature")]
public abstract class MessageUpdatedContent
{ }

public class SignatureUpdatedContent : MessageUpdatedContent
{
    public required string Signature { get; set; }
}
