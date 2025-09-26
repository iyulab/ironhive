using IronHive.Abstractions.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion.Models;

public class StreamingChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public IEnumerable<ChatChoiceDelta>? Choices { get; set; }

    [JsonPropertyName("created")]
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime Created { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// always "chat.completion.chunk"
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; } = "chat.completion.chunk";

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }

    [JsonPropertyName("usage")]
    public ChatTokenUsage? Usage { get; set; }
}
