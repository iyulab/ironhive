namespace IronHive.Abstractions.ChatCompletion.Messages;

public enum MessageRole
{
    User,
    Assistant
}

public class Message
{
    public MessageRole Role { get; set; }

    public string? Name { get; set; }

    public MessageContentCollection Content { get; set; } = new();

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Message()
    { }

    public Message(MessageRole role)
    {
        Role = role;
    }
}
