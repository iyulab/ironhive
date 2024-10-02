namespace Raggle.Abstractions.Models;

public enum StreamingSystemStatus
{
    Limit,
    Stop,
    Error,
}

public enum StreamingTextStatus
{
    TextBegin,
    TextEnd,
}

public enum StreamingToolStatus
{
    ToolCall,
    ToolUse,
    ToolResult,
}

public enum StreamingMemoryStatus
{
    MemorySearch,
    MemoryResult,
}

public interface IStreamingChatResponse
{
    DateTime TimeStamp { get; set; }
}

public abstract class StreamingChatResponseBase : IStreamingChatResponse
{
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
}

public class StreamingSystemResponse : StreamingChatResponseBase
{
    public StreamingSystemStatus Status { get; set; }
    public string? Message { get; set; }
}

public class StreamingMemoryResponse : StreamingChatResponseBase
{
    public StreamingMemoryStatus Status { get; set; }
    public string? Query { get; set; }
    public string? Files { get; set; }
}

public class StreamingTextResponse : StreamingChatResponseBase
{
    public StreamingTextStatus Status { get; set; }
    public string? Text { get; set; }
}

public class StreamingToolResponse : StreamingChatResponseBase
{
    public StreamingToolStatus Status { get; set; }
    public string? Name { get; set; }
    public string? Arguments { get; set; }
    public string? Result { get; set; }
}
