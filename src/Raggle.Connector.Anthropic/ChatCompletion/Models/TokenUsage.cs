using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

internal class TokenUsage
{
    [JsonPropertyName("input_tokens")]
    internal int InputTokens { get; set; }

    [JsonPropertyName("cache_creation_input_tokens")]
    internal int? CacheCreateTokens { get; set; }

    [JsonPropertyName("cache_read_input_tokens")]
    internal int? CacheReadTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    internal int OutputTokens { get; set; }
}
