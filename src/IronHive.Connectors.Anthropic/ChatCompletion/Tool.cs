using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

internal class Tool
{
    /// <summary>
    /// "custom", 
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    public required ToolInputSchema InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

internal class ToolInputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; } = "object";

    [JsonPropertyName("properties")]
    public object? Properties { get; set; }

    [JsonPropertyName("required")]
    public IEnumerable<string>? Required { get; set; }
}
