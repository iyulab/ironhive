using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

internal class Tool
{
    [JsonPropertyName("name")]
    internal required string Name { get; set; }

    [JsonPropertyName("description")]
    internal string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    internal required InputSchema InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    internal CacheControl? CacheControl { get; set; }
}

internal class InputSchema
{
    /// <summary>
    /// "object" only
    /// </summary>
    [JsonPropertyName("type")]
    internal string Type { get; } = "object";

    [JsonPropertyName("properties")]
    internal object? Properties { get; set; }

    [JsonPropertyName("required")]
    internal string[]? Required { get; set; }
}
