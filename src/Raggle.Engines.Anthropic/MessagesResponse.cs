using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageResponseType
{
    message,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MessageResponseRole
{
    assistant
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StopReason
{
    end_turn,
    max_tokens,
    stop_sequence,
    tool_use
}

public enum MessageResponseContentType
{

}

public abstract class MessageResponseContent
{
    abstract public string Type
    {
        get;
    }
}

public class MessagesResponse
{
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public MessageResponseType Type { get; set; }

    [JsonPropertyName("role")]
    public MessageResponseRole Role { get; set; }

    [JsonPropertyName("content")]
    public object[] Content { get; set; } = [];

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("stop_reason")]
    public StopReason? StopReason { get; set; }

    [JsonPropertyName("stop_sequence")]
    public string? StopSequence { get; set; }

    [JsonPropertyName("usage")]
    public required TokenUsage Usage { get; set; }
}

public class TokenUsage
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