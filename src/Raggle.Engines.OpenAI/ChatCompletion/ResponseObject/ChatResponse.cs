using System.Text.Json.Serialization;
using Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatResponseObject;

public class ChatResponse
{
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [JsonPropertyName("choices")]
    public ICollection<ChatChoice> Choices { get; set; } = [];

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string SystemFingerprint { get; set; } = string.Empty;

    // always "chat.completion"
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("usage")]
    public TokenUsage Usage { get; set; } = new();
}

public class ChatChoice
{
    // "stop", "length", "content_filter", "tool_calls"
    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public object Message { get; set; } = new();
}

