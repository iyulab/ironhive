using Raggle.Abstractions.Tools;

namespace Raggle.Abstractions.Models;

public enum ContentType
{
    Text,
    Image,
    Tool,
    File,
    Memory
}

public interface IUserContent
{
    ContentType Type { get; }
}

public interface IAssistantContent
{
    ContentType Type { get; }
}

// For User, Assistant
public class TextContent : IUserContent, IAssistantContent
{
    public ContentType Type => ContentType.Text;

    public string? Text { get; set; }
}

// For User
public class FileContent : IUserContent
{
    public ContentType Type => ContentType.File;

    public string? FileName { get; set; }

    public string? Extension { get; set; }

    public string? MimeType { get; set; }

    public long? Size { get; set; }

    public string? Data { get; set; }

    public string? Url { get; set; }
}

// For Assistant
public class ImageContent : IAssistantContent
{
    public ContentType Type => ContentType.Image;

    public string? Data { get; set; }

    public string? Url { get; set; }
}

// For Assistant
public class ToolContent : IAssistantContent
{
    public ContentType Type => ContentType.Tool;

    public string? ID { get; set; }

    public string? Name { get; set; }

    public object? Arguments { get; set; }

    public FunctionResult? Result { get; set; }
}

// For Assistant
public class MemoryContent : IAssistantContent
{
    public ContentType Type => ContentType.Memory;

    public string? Index { get; set; }

    public string? FileName { get; set; }

    public string? Partition { get; set; }

    public string? Segment { get; set; }

    public object? MetaData { get; set; }
}
