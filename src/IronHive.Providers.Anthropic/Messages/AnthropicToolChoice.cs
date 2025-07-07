using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(AutoAnthropicToolChoice), "auto")]
[JsonDerivedType(typeof(NoneAnthropicToolChoice), "none")]
[JsonDerivedType(typeof(AnyAnthropicToolChoice), "any")]
[JsonDerivedType(typeof(ManualAnthropicToolChoice), "tool")]
internal abstract class AnthropicToolChoice
{ }

internal class AutoAnthropicToolChoice : AnthropicToolChoice 
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class NoneAnthropicToolChoice : AnthropicToolChoice
{ }

internal class AnyAnthropicToolChoice : AnthropicToolChoice 
{
    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}

internal class ManualAnthropicToolChoice : AnthropicToolChoice
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("disable_parallel_tool_use")]
    public bool? DisaableParallelToolUse { get; set; }
}
