using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

internal enum StopReason
{
    EndTurn,
    MaxTokens,
    StopSequence,
    ToolUse
}

internal class MessagesResponse
{
    /// <summary>
    /// text or tool content
    /// </summary>
    [JsonPropertyName("content")]
    public required IEnumerable<MessageContent> Content { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// "assistant" only
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; } = "assistant";

    /// <summary>
    /// "end_turn", "max_tokens", "stop_sequence", "tool_use"
    /// </summary>
    [JsonPropertyName("stop_reason")]
    public StopReason? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    public string? StopSequence { get; set; }

    /// <summary>
    /// "message" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "message";

    [JsonPropertyName("usage")]
    public required TokenUsage Usage { get; set; }
}
