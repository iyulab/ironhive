namespace Raggle.Abstractions.Models;

public enum MessageRole
{
    User,
    Assistant
}

public interface IMessage
{
    MessageRole Role { get; }

    DateTime CreatedAt { get; set; }
}

public abstract class MessageBase : IMessage
{
    public abstract MessageRole Role { get; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class UserMessage : MessageBase
{
    public override MessageRole Role => MessageRole.User;

    public ICollection<IUserContent> Contents { get; set; } = [];
}

public class AssistantMessage : MessageBase
{
    public override MessageRole Role => MessageRole.Assistant;

    public ICollection<IAssistantContent> Contents { get; set; } = [];
}
