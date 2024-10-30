using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(AnyToolChoice), "any")]
[JsonDerivedType(typeof(ManualToolChoice), "tool")]
internal abstract class ToolChoice { }

internal class AutoToolChoice : ToolChoice { }

internal class AnyToolChoice : ToolChoice { }

internal class ManualToolChoice : ToolChoice
{
    [JsonPropertyName("name")]
    internal required string Name { get; set; }
}
