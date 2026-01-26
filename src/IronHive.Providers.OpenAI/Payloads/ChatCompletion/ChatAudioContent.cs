using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

public class ChatAudioContent
{
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("expires_at")]
    public int? ExpiresAt { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("transcript")]
    public string? Transcript { get; set; }
}
