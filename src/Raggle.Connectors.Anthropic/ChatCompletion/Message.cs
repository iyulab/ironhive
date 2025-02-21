using System.Text.Json.Serialization;

namespace Raggle.Connectors.Anthropic.ChatCompletion;

internal enum MessageRole
{
    User,
    Assistant
}

internal class Message
{
    /// <summary>
    /// "user" or "assistant"
    /// </summary>
    [JsonPropertyName("role")]
    public required MessageRole Role { get; set; }

    [JsonPropertyName("content")]
    public required ICollection<MessageContent> Content { get; set; }
}
