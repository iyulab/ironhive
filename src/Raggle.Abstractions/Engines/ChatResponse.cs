namespace Raggle.Abstractions.Engines;

public enum ChatResponseState
{
    Stop,
    Limit,
    Error,
}

public class ChatResponse
{
    public ChatResponseState State { get; set; }

    public IContentBlock[] Contents { get; set; } = [];

    public int? TotalTokens { get; set; }

    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

    public string? Message { get; set; }

    public static ChatResponse Stop(IContentBlock[] contents, int? totalTokens = null)
    {
        return new ChatResponse
        {
            State = ChatResponseState.Stop,
            Contents = contents,
            TotalTokens = totalTokens
        };
    }

    public static ChatResponse Limit(IContentBlock[] contents, int? totalTokens = null)
    {
        return new ChatResponse
        {
            State = ChatResponseState.Limit,
            Contents = contents,
            TotalTokens = totalTokens
        };
    }

    public static ChatResponse Error(string message)
    {
        return new ChatResponse
        {
            State = ChatResponseState.Error,
            Message = message
        };
    }
}
