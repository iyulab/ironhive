using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

internal class MessagesRequest
{
    [JsonPropertyName("max_tokens")]
    public required int MaxTokens { get; set; }

    [JsonPropertyName("messages")]
    public required IEnumerable<Message> Messages { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("metadata")]
    public object? Metadata { get; set; }

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

    /// <summary>
    /// (Not Use)
    /// </summary>
    [JsonPropertyName("thinking")]
    public object? Thinking { get; set; }

    [JsonPropertyName("tool_choice")]
    public ToolChoice? ToolChoice { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<Tool>? Tools { get; set; }

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
