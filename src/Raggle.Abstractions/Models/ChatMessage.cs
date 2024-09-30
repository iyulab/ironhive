namespace Raggle.Abstractions.Models;

public enum MessageRole
{
    User,
    Assistant
}

public class ChatMessage
{
    public required MessageRole Role { get; set; }

    public ICollection<ContentBlock> Contents { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
