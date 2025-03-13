using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(AnyToolChoice), "any")]
[JsonDerivedType(typeof(ManualToolChoice), "tool")]
[JsonDerivedType(typeof(NoneToolChoice), "none")]
internal abstract class ToolChoice
{ }

internal class NoneToolChoice : ToolChoice
{ }

internal class AutoToolChoice : ToolChoice 
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class AnyToolChoice : ToolChoice 
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class ManualToolChoice : ToolChoice
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}
