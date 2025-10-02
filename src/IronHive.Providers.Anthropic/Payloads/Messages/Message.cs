using System.Text.Json.Serialization;

namespace IronHive.Providers.Anthropic.Payloads.Messages;

internal enum MessageRole
{
    User,
    Assistant
}

internal class Message
{
    [JsonPropertyName("role")]
    public MessageRole Role { get; set; }

    [JsonPropertyName("content")]
    public ICollection<MessageContent> Content { get; set; } = [];

    public Message()
    { }

    public Message(MessageRole role)
    {
        Role = role;
    }
}