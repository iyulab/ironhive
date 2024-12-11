using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public class ChatCompletionStreamingResponse
{
    public ChatCompletionEndReason? EndReason { get; set; }

    public string? Model { get; set; }

    public IMessageContent? Content { get; set; }

    public TokenUsage? TokenUsage { get; set; }
}
