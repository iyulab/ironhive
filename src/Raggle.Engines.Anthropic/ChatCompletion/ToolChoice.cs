using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(AnyToolChoice), "any")]
[JsonDerivedType(typeof(ManualToolChoice), "tool")]
public abstract class ToolChoice { }

public class AutoToolChoice : ToolChoice { }

public class AnyToolChoice : ToolChoice { }

public class ManualToolChoice : ToolChoice
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
