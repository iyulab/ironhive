using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

public class ChatCompletionResponse: JsonPayloadResponse
{
    [JsonPropertyName("choices")]
    public IEnumerable<ChatChoice>? Choices { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// always "chat.completion"
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "chat.completion";

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("usage")]
    public ChatTokenUsage? Usage { get; set; }
}
