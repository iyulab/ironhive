using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

internal class MessagesUsage
{
    [JsonPropertyName("cache_creation")]
    public CacheCreation? CacheCreation { get; set; }

    [JsonPropertyName("cache_creation_input_tokens")]
    public int? CacheCreateTokens { get; set; }

    [JsonPropertyName("cache_read_input_tokens")]
    public int? CacheReadTokens { get; set; }

    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    /// <summary>
    /// tool use count like "web_search"
    /// </summary>
    [JsonPropertyName("server_tool_use")]
    public object? ServerToolUse { get; set; }

    /// <summary>
    /// "standard", "priority", "batch"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }
}

internal class CacheCreation
{
    [JsonPropertyName("ephemeral_1h_input_tokens")]
    public int? Ephemeral1hInputTokens { get; set; }

    [JsonPropertyName("ephemeral_5m_input_tokens")]
    public int? Ephemeral5mInputTokens { get; set; }
}
