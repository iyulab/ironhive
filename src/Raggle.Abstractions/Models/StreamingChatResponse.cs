namespace Raggle.Abstractions.Models;

public enum StreamingStatus
{
    Begin,
    InProgress,
    End,
    Error,
}

public class StreamingChatResponse
{
    public StreamingStatus Status { get; set; }
    public ContentBlock? BlockChunk { get; set; }
    public string? ErrorMessage { get; set; }

    public StreamingChatResponse(StreamingStatus status)
    {
        Status = status;
    }

    public static StreamingChatResponse Error(string? errorMessage)
    {
        return new StreamingChatResponse(StreamingStatus.Error)
        {
            ErrorMessage = errorMessage
        };
    }
}