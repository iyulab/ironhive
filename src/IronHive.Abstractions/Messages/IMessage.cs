using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// Interface for messages.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
public interface IMessage
{
    /// <summary>
    /// name of message owner.
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// when the message was created.
    /// </summary>
    DateTime Timestamp { get; set; }
}

/// <summary>
/// Base class for messages.
/// </summary>
public abstract class MessageBase : IMessage
{
    /// <inheritdoc />
    public string? Name { get; set; }

    /// <inheritdoc />
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// User message.
/// </summary>
public class UserMessage : MessageBase
{
    /// <summary>
    /// User message content. "text" or "file".
    /// </summary>
    public UserContentCollection Content { get; set; } = new();
}

/// <summary>
/// Assistant message.
/// </summary>
public class AssistantMessage : MessageBase
{
    /// <summary>
    /// Assistant message content. "thinking", "text" or "tool".
    /// </summary>
    public AssistantContentCollection Content { get; set; } = new();
}

