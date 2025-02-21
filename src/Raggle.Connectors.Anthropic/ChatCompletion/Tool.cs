using System.Text.Json.Serialization;

namespace Raggle.Connectors.Anthropic.ChatCompletion;

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
    public required object InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}
