namespace Raggle.Abstractions.Messages;

/// <summary>
/// Represent a message.
/// </summary>
public class Message
{
    /// <summary>
    /// message role.
    /// </summary>
    public MessageRole Role { get; set; }

    /// <summary>
    /// the name of the user or assistant.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// the content of the message.
    /// </summary>
    public MessageContentCollection Content { get; set; } = new MessageContentCollection();

    /// <summary>
    /// created timestamp.
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
