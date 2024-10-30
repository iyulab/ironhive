using Raggle.Abstractions.Messages;

namespace Raggle.Abstractions.AI;

public enum ChatResponseState
{
    Stop,
    Limit,
    Error,
}

public class ChatCompletionResponse
{
    public ChatResponseState State { get; set; }

    public IContentBlock[] Contents { get; set; } = [];

    public int? TotalTokens { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public string? Message { get; set; }

    public static ChatCompletionResponse Stop(IContentBlock[] contents, int? totalTokens = null)
    {
        return new ChatCompletionResponse
        {
            State = ChatResponseState.Stop,
            Contents = contents,
            TotalTokens = totalTokens
        };
    }

    public static ChatCompletionResponse Limit(IContentBlock[] contents, int? totalTokens = null)
    {
        return new ChatCompletionResponse
        {
            State = ChatResponseState.Limit,
            Contents = contents,
            TotalTokens = totalTokens
        };
    }

    public static ChatCompletionResponse Error(string message)
    {
        return new ChatCompletionResponse
        {
            State = ChatResponseState.Error,
            Message = message
        };
    }
}
