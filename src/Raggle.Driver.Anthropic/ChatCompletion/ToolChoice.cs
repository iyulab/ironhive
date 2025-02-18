using System.Text.Json.Serialization;

namespace Raggle.Driver.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(AnyToolChoice), "any")]
[JsonDerivedType(typeof(ManualToolChoice), "tool")]
internal abstract class ToolChoice
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class AutoToolChoice : ToolChoice { }

internal class AnyToolChoice : ToolChoice { }

internal class ManualToolChoice : ToolChoice
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
