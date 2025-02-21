using System.Text.Json.Serialization;

namespace Raggle.Connectors.Anthropic.ChatCompletion;

internal class TokenUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("cache_creation_input_tokens")]
    public int? CacheCreateTokens { get; set; }

    [JsonPropertyName("cache_read_input_tokens")]
    public int? CacheReadTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }
}
