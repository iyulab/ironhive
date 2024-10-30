using System.Text.Json.Serialization;

namespace Raggle.Connector.Anthropic.ChatCompletion.Models;

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
    internal required string ID { get; set; }

    /// <summary>
    /// "message" only
    /// </summary>
    [JsonPropertyName("type")]
    internal string Type { get; } = "message";

    /// <summary>
    /// "assistant" only
    /// </summary>
    [JsonPropertyName("role")]
    internal string Role { get; } = "assistant";

    [JsonPropertyName("model")]
    internal required string Model { get; set; }

    /// <summary>
    /// <see cref="MessageTextContent"/> or <see cref="MessageToolUseContent"/>
    /// </summary>
    [JsonPropertyName("content")]
    internal required MessageContent[] Content { get; set; }

    /// <summary>
    /// "end_turn", "max_tokens", "stop_sequence", "tool_use"
    /// </summary>
    [JsonPropertyName("stop_reason")]
    internal StopReason? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    internal string? StopSequence { get; set; }

    [JsonPropertyName("usage")]
    internal required TokenUsage Usage { get; set; }
}
