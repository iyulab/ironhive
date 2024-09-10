namespace Raggle.Abstractions.Messages;

public class ChatSession
{
    public string SystemMessage { get; set; } = string.Empty;
    public ChatHistory ChatHistory { get; set; } = new();
    public TokenUsage TokenUsage { get; set; } = new();
}
