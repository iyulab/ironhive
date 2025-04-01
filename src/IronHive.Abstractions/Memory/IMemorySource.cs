using System.Text.Json.Serialization;
using MessagePack;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// "text", "file", "web" 중 하나의 소스를 나타내는 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMemorySource), "text")]
[JsonDerivedType(typeof(FileMemorySource), "file")]
[JsonDerivedType(typeof(WebMemorySource), "web")]
[Union(0, typeof(TextMemorySource))]
[Union(1, typeof(FileMemorySource))]
[Union(2, typeof(WebMemorySource))]
public interface IMemorySource
{
    string Id { get; set; }
}

public abstract class MemorySourceBase : IMemorySource
{
    [Key(0)]
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// 텍스트 데이터를 나타내는 소스입니다.
/// </summary>
[MessagePackObject]
public class TextMemorySource : MemorySourceBase
{
    [Key(1)]
    public required string Text { get; set; }
}

/// <summary>
/// 지정된 파일 스토리지의 데이터를 나타내는 소스입니다.
/// </summary>
[MessagePackObject]
public class FileMemorySource : MemorySourceBase
{
    [Key(1)]
    public required string Provider { get; set; }

    [Key(2)]
    public object? ProviderConfig { get; set; }

    [Key(3)]
    public required string FilePath { get; set; }

    [Key(4)]
    public string? MimeType { get; set; }

    [Key(5)]
    public long? Size { get; set; }
}

/// <summary>
/// 지정된 URL을 나타내는 소스입니다.
/// </summary>
[MessagePackObject]
public class WebMemorySource : MemorySourceBase
{
    [Key(1)]
    public required string Url { get; set; }
}
