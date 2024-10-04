namespace Raggle.Abstractions.Models;

public enum ChatResponseState
{
    Stop,
    Limit,
    Error,
}

public class ChatResponse
{
    public ChatResponseState State { get; set; }

    public IContentBlock[] ContentBlock { get; set; } = [];

    public int? TotalTokens { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Message { get; set; }

    public static ChatResponse Stop(IContentBlock[] contentBlock, int? totalTokens = null)
    {
        return new ChatResponse
        {
            State = ChatResponseState.Stop,
            ContentBlock = contentBlock,
            TotalTokens = totalTokens
        };
    }

    public static ChatResponse Limit(IContentBlock[] contentBlock, int? totalTokens = null)
    {
        return new ChatResponse
        {
            State = ChatResponseState.Limit,
            ContentBlock = contentBlock,
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
