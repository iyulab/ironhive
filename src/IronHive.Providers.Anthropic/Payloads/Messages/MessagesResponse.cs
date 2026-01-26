using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

internal enum StopReason
{
    EndTurn,
    MaxTokens,
    StopSequence,
    ToolUse,
    PauseTurn,
    Refusal
}

internal class MessagesResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    /// <summary>
    /// text or tool content
    /// </summary>
    [JsonPropertyName("content")]
    public required IEnumerable<MessageContent> Content { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    /// <summary>
    /// "assistant" always
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; } = "assistant";

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
    public required MessagesUsage Usage { get; set; }
}
