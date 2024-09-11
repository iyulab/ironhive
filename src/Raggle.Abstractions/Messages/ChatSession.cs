namespace Raggle.Abstractions.Messages;

public class ChatSession
{
    public string? SystemMessage { get; set; }
    public ICollection<IMessage> Messages { get; set; } = [];
    public int TotalToken { get; set; }

    public void AddMessage(IMessage message)
    {
        Messages.Add(message);
    }

    public void AddSystemMessage(string message)
    {
        SystemMessage = message;
    }

    public void AddUserMessage(string text, ICollection<ImageContent> images)
    {
        Messages.Add(new UserMessage
        {
            Text = text,
            Images = images
        });
    }

    public void AddAssistantMessage(string text, ICollection<ToolContent> tools)
    {
        Messages.Add(new AssistantMessage
        {
            Text = text,
            Tools = tools
        });
    }
}
