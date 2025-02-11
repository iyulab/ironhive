using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public class MessageResponse
{
    public MessageEndReason? EndReason { get; set; }

    public string? Model { get; set; }

    public Message? Message { get; set; }

    public TokenUsage? TokenUsage { get; set; }
}
