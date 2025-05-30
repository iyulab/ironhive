using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Messages;

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
    public ICollection<IMessageContent> Content { get; set; } = [];

    public Message()
    { }

    public Message(MessageRole role)
    {
        Role = role;
    }
}
