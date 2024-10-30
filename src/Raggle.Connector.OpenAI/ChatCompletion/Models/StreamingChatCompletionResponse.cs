using Raggle.Connector.OpenAI.Converters;
using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class StreamingChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("choices")]
    public ChoiceDelta[]? Choices { get; set; }

    [JsonPropertyName("created")]
    [JsonConverter(typeof(UnixTimeJsonConverter))]
    public DateTime Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("service_tier")]
    public string? ServiceTier { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }

    /// <summary>
    /// "chat.completion" only
    /// </summary>
    [JsonPropertyName("object")]
    public string? Object { get; } = "chat.completion";

    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}
