using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoToolChoice), "auto")]
[JsonDerivedType(typeof(AnyToolChoice), "any")]
[JsonDerivedType(typeof(ManualToolChoice), "tool")]
[JsonDerivedType(typeof(NoneToolChoice), "none")]
internal interface IToolChoice
{ }

internal class NoneToolChoice : IToolChoice
{ }

internal class AutoToolChoice : IToolChoice 
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class AnyToolChoice : IToolChoice 
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class ManualToolChoice : IToolChoice
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}
