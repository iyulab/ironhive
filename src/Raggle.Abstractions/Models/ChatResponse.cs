namespace Raggle.Abstractions.Models;

public class ChatResponse
{
    public bool IsSuccess { get; set; } = true;
    public ContentBlock? ContentBlock { get; set; }
    public TokenUsage? TokenUsage { get; set; }
    public string? ErrorMessage { get; set; }

    public static ChatResponse Success(ContentBlock contentBlock, TokenUsage? tokenUsage = null)
    {
        return new ChatResponse
        {
            IsSuccess = true,
            ContentBlock = contentBlock,
            TokenUsage = tokenUsage
        };
    }

    public static ChatResponse Failed(string errorMessage)
    {
        return new ChatResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public class TokenUsage
{
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public int? TotalTokens => InputTokens + OutputTokens;
}
