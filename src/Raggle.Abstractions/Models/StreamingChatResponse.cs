namespace Raggle.Abstractions.Models;

public enum StreamingStatus
{
    TextGeneration,
    ToolCall,
    Stop,
    Error,
}

public class StreamingChatResponse
{
    public StreamingStatus Status { get; set; }
    public ContentBlock? ChunkBlock { get; set; }
    public string? ErrorMessage { get; set; }

    public static StreamingChatResponse Stop()
    {
        return new StreamingChatResponse
        {
            Status = StreamingStatus.Stop
        };
    }

    public static StreamingChatResponse Error(string? errorMessage)
    {
        return new StreamingChatResponse
        {
            Status = StreamingStatus.Error,
            ErrorMessage = errorMessage
        };
    }

    public static StreamingChatResponse Text(TextContentBlock textBlock)
    {
        return new StreamingChatResponse
        {
            Status = StreamingStatus.TextGeneration,
            ChunkBlock = textBlock
        };
    }

    public static StreamingChatResponse Tool(ToolContentBlock toolBlock)
    {
        return new StreamingChatResponse
        {
            Status = StreamingStatus.ToolCall,
            ChunkBlock = toolBlock
        };
    }
}