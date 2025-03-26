using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "role")]
[JsonDerivedType(typeof(UserMessage), "user")]
[JsonDerivedType(typeof(AssistantMessage), "assistant")]
public interface IMessage
{
    public string? Name { get; set; }

    DateTime Timestamp { get; set; }
}

public abstract class MessageBase : IMessage
{
    public string? Name { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class UserMessage : MessageBase
{
    public UserContentCollection Content { get; set; } = new();
}

public class AssistantMessage : MessageBase
{
    public AssistantContentCollection Content { get; set; } = new();
}
