using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

/// <summary>
/// Beta 기능들을 제외한 Anthropic 메시지 요청 페이로드입니다.
/// </summary>
internal class MessagesRequest
{
    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    public required ICollection<Message> Messages { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("metadata")]
    public UserMetadata? Metadata { get; set; }

    /// <summary>
    /// "auto", "standard_only"
    /// </summary>
    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("stop_sequences")]
    public IEnumerable<string>? StopSequences { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("system")]
    public string? System { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("thinking")]
    public ThinkingConfig? Thinking { get; set; }

    [JsonPropertyName("tool_choice")]
    public AnthropicToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<AnthropicTool>? Tools { get; set; }

    /// <summary>
    /// 0 to 100
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; set; }

    /// <summary>
    /// 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }
}
