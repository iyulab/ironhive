using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class Tool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    public required InputSchema InputSchema { get; set; }

    [JsonPropertyName("cache_control")]
    public CacheControl? CacheControl { get; set; }
}

public class InputSchema
{
    /// <summary>
    /// "object" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "object";

    [JsonPropertyName("properties")]
    public object? Properties { get; set; }

    [JsonPropertyName("required")]
    public ICollection<string>? Required { get; set; }
}
