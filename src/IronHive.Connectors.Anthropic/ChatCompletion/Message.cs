using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic.ChatCompletion;

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
    public MessageRole Role { get; set; }

    [JsonPropertyName("content")]
    public ICollection<MessageContent> Content { get; set; } = new List<MessageContent>();

    public Message()
    { }

    public Message(MessageRole role)
    {
        Role = role;
    }
}
