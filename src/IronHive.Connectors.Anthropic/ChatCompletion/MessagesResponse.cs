using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

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
    /// <summary>
    /// (Not Use) Information about the container that the tool is running in.
    /// </summary>
    [JsonPropertyName("container")]
    public object? Container { get; set; }

    /// <summary>
    /// text or tool content
    /// </summary>
    [JsonPropertyName("content")]
    public required IEnumerable<IMessageContent> Content { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

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
