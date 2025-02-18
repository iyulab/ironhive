using System.Text.Json.Serialization;

namespace Raggle.Driver.Anthropic.ChatCompletion;

internal enum StopReason
{
    EndTurn,
    MaxTokens,
    StopSequence,
    ToolUse
}

internal class MessagesResponse
{
    [JsonPropertyName("id")]
    public required string ID { get; set; }

    /// <summary>
    /// "message" only
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; } = "message";

    /// <summary>
    /// "assistant" only
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; } = "assistant";

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageToolUseContent"/>
    /// </summary>
    [JsonPropertyName("content")]
    public required IEnumerable<MessageContent> Content { get; set; }

    /// <summary>
    /// "end_turn", "max_tokens", "stop_sequence", "tool_use"
    /// </summary>
    [JsonPropertyName("stop_reason")]
    public StopReason? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    public string? StopSequence { get; set; }

    [JsonPropertyName("usage")]
    public required TokenUsage Usage { get; set; }
}
