namespace Raggle.Abstractions.ChatCompletion.Messages;

public class MessageContext
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public MessageCollection Messages { get; set; } = new();

    public int MaxLoopCount { get; set; } = 3;

    public int TokenUsageCount { get; set; } = 0;

    public MessageContext(IEnumerable<Message> messages)
    {
        Messages = new(messages);
    }
}
