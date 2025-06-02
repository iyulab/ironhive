using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// "text", "file", "web" 중 하나의 소스를 나타내는 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMemorySource), "text")]
[JsonDerivedType(typeof(FileMemorySource), "file")]
[JsonDerivedType(typeof(WebMemorySource), "web")]
public interface IMemorySource
{
    string Id { get; set; }
}

/// <summary>
/// 메모리 소스의 기본 클래스입니다.
/// </summary>
public abstract class MemorySourceBase : IMemorySource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// 텍스트 데이터를 나타내는 소스입니다.
/// </summary>
public class TextMemorySource : MemorySourceBase
{
    public required string Value { get; set; }
}

/// <summary>
/// 지정된 파일 스토리지의 데이터를 나타내는 소스입니다.
/// </summary>
public class FileMemorySource : MemorySourceBase
{
    public required string Storage { get; set; }

    public required string FilePath { get; set; }

    public string? MimeType { get; set; }

    public long? FileSize { get; set; }
}

/// <summary>
/// 지정된 URL을 나타내는 소스입니다.
/// </summary>
public class WebMemorySource : MemorySourceBase
{
    public required string Url { get; set; }
}
