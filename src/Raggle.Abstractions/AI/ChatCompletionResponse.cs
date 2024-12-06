using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public class ChatCompletionResponse
{
    public required bool Completed { get; set; }

    public IMessageContent[] Content { get; set; } = [];

    public TokenUsage? TokenUsage { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}
