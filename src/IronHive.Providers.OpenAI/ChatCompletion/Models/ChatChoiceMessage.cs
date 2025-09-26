using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion.Models;

public class ChatChoiceMessage
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// when using web search tools, urls are returned.
    /// </summary>
    [JsonPropertyName("annotations")]
    public object? Annotations { get; set; }

    /// <summary>
    /// If the audio output modality is requested
    /// </summary>
    [JsonPropertyName("audio")]
    public AudioContent? Audio { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCall>? ToolCalls { get; set; }

    public class AudioContent
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
}

public class ChatChoiceMessageDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("refusal")]
    public string? Refusal { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    /// <summary>
    /// the tools that the assistant calls.
    /// </summary>
    [JsonPropertyName("tool_calls")]
    public ICollection<ChatToolCallDelta>? ToolCalls { get; set; }
}