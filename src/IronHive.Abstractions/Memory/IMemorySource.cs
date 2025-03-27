using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Memory;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMemorySource), "text")]
//[JsonDerivedType(typeof(WebMemorySource), "web")]
[JsonDerivedType(typeof(FileMemorySource), "file")]
public interface IMemorySource
{
    string Id { get; set; }
}

public abstract class MemorySourceBase : IMemorySource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

public class TextMemorySource : MemorySourceBase
{
    public required string Text { get; set; }
}

//public class WebMemorySource : MemorySourceBase
//{
//    public required HttpMethod Method { get; set; }
//    public required string Url { get; set; }
//    public IDictionary<string, string>? Query { get; set; }
//    public IDictionary<string, string>? Headers { get; set; }
//    public string? Body { get; set; }
//}

public class FileMemorySource : MemorySourceBase
{
    public required string StorageType { get; set; }

    public required object StorageConfig { get; set; }

    public required string FilPath { get; set; }

    public string? MimeType { get; set; }

    public long? Size { get; set; }

    // TODO: This should be removed
    public Stream? Data { get; set; }
}
