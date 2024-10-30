using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    internal string? ID { get; set; }

    [JsonPropertyName("choices")]
    internal Choice[]? Choices { get; set; }

    [JsonPropertyName("created")]
    internal int Created { get; set; }

    [JsonPropertyName("model")]
    internal string? Model { get; set; }

    [JsonPropertyName("service_tier")]
    internal string? ServiceTier { get; set; }

    [JsonPropertyName("system_fingerprint")]
    internal string? SystemFingerprint { get; set; }

    /// <summary>
    /// "chat.completion" only
    /// </summary>
    [JsonPropertyName("object")]
    internal string Object { get; } = "chat.completion";

    [JsonPropertyName("usage")]
    internal TokenUsage? Usage { get; set; }
}
