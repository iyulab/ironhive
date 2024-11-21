using System.Text.Json.Serialization;

namespace Raggle.Driver.OpenAI.ChatCompletion.Models;

internal class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("choices")]
    public Choice[]? Choices { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

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
    public string Object { get; } = "chat.completion";

    [JsonPropertyName("usage")]
    public TokenUsage? Usage { get; set; }
}
