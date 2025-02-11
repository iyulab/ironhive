using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public class StreamingMessageResponse
{
    public MessageEndReason? EndReason { get; set; }

    public string? Model { get; set; }

    public IMessageContent? Content { get; set; }

    public TokenUsage? TokenUsage { get; set; }
}
