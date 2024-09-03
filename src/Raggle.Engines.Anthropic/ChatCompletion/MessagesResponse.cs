using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class MessagesResponse
{
    [JsonPropertyName("id")]
    public required string ID { get; set; }

    // always "message"
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    // always "assistant"
    [JsonPropertyName("role")]
    public required string Role { get; set; }

    [JsonPropertyName("content")]
    public required ICollection<Message> Content { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    // "end_turn", "max_tokens", "stop_sequence", "tool_use"
    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    public string? StopSequence { get; set; }

    [JsonPropertyName("usage")]
    public required TokenUsage Usage { get; set; }
}
