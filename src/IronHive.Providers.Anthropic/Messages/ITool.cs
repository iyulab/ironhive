using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages;

/// <summary>
/// Custom tool only, not use other tools
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CustomTool), "custom")]
internal interface ITool
{ }

internal class CustomTool : ITool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    public required object InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}
