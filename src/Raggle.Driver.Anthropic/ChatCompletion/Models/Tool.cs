using System.Text.Json.Serialization;

namespace Raggle.Driver.Anthropic.ChatCompletion.Models;

internal class Tool
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
