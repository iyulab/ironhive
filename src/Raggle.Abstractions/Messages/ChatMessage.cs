namespace Raggle.Abstractions.Messages;

public enum MessageRole
{
    User,
    Assistant
}

public interface IMessage
{
    MessageRole Role { get; }
}

public class UserMessage : IMessage
{
    public MessageRole Role => MessageRole.User;
    public string? Text { get; set; }
    public ICollection<ImageContent>? Images { get; set; }
}

public class AssistantMessage : IMessage
{
    public MessageRole Role => MessageRole.Assistant;
    public string? Text { get; set; }
    public ICollection<ToolContent>? Tools { get; set; }
}

public class ImageContent
{
    public string? Data { get; set; }
}

public class ToolContent
{
    public string? Name { get; set; }
    public object? Arguments { get; set; }
    public object? Result { get; set; }
}
