namespace IronHive.Abstractions.Message.Roles;

/// <summary>
/// User message.
/// </summary>
public class UserMessage : Message
{
    /// <summary>
    /// User message content. "text" or "file".
    /// </summary>
    public ICollection<MessageContent> Content { get; set; } = [];

    /// <summary>
    /// Timestamp of the message.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
