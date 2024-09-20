using Raggle.Abstractions.Models;

namespace Raggle.Abstractions.Engines;

public interface IChatCompletionEngine
{
    Task<IEnumerable<ChatCompletionModel>> GetChatCompletionModelsAsync();
    Task<ChatCompletionResponse> ChatCompletionAsync(ChatSession session, ChatCompletionOptions options);
    IAsyncEnumerable<StreamingChatCompletionResponse> StreamingChatCompletionAsync(ChatSession session, ChatCompletionOptions options);
}

public class ChatCompletionModel
{
    public required string ModelId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Owner { get; set; }
}

public class ChatCompletionResponse
{
    public bool IsSuccess { get; set; } = true;
    public ContentBlock? ContentBlock { get; set; }
    public TokenUsage? TokenUsage { get; set; }
    public string? ErrorMessage { get; set; }

    public static ChatCompletionResponse Success(ContentBlock contentBlock, TokenUsage? tokenUsage = null)
    {
        return new ChatCompletionResponse
        {
            IsSuccess = true,
            ContentBlock = contentBlock,
            TokenUsage = tokenUsage
        };
    }

    public static ChatCompletionResponse Failed(string errorMessage)
    {
        return new ChatCompletionResponse
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public enum StreamingStatus
{
    Error,
    Start,
    ToolStart,
    ToolProgress,
    ToolFinish,
    TextStart,
    TextProgress,
    TextFinish,
    Stop,
}

public class StreamingChatCompletionResponse
{
    public StreamingStatus Status { get; set; }
    public ContentBlock? BlockChunk { get; set; }
    public string? ErrorMessage { get; set; }

    public static StreamingChatCompletionResponse Error(string? errorMessage)
    {
        return new StreamingChatCompletionResponse
        {
            Status = StreamingStatus.Error,
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
