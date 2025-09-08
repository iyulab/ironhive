using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Memory;

/// <summary>
/// "text", "file", "web" 중 하나의 소스를 나타내는 객체입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextMemorySource), "text")]
[JsonDerivedType(typeof(FileMemorySource), "file")]
[JsonDerivedType(typeof(WebMemorySource), "web")]
public interface IMemorySource
{
    /// <summary>
    /// 소스의 고유 식별자를 나타냅니다.
    /// </summary>
    string Id { get; set; }
}

/// <inheritdoc />
public abstract class MemorySourceBase : IMemorySource
{
    /// <inheritdoc />
    public string Id { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// 텍스트 데이터를 나타내는 소스입니다.
/// </summary>
public class TextMemorySource : MemorySourceBase
{
    /// <summary>
    /// 소스의 텍스트 값을 나타냅니다.
    /// </summary>
    public required string Value { get; set; }
}

/// <summary>
/// 지정된 파일 스토리지의 데이터를 나타내는 소스입니다.
/// </summary>
public class FileMemorySource : MemorySourceBase
{
    /// <summary>
    /// 파일이 저장된 스토리지의 이름을 나타냅니다.
    /// </summary>
    public required string StorageName { get; set; }

    /// <summary>
    /// 파일의 경로를 나타냅니다. 이 경로는 스토리지 내에서 파일을 식별하는 데 사용됩니다.
    /// </summary>
    public required string FilePath { get; set; }
}

/// <summary>
/// 지정된 URL을 나타내는 소스입니다.
/// </summary>
public class WebMemorySource : MemorySourceBase
{
    /// <summary>
    /// 웹 소스의 URL을 나타냅니다. 이 URL은 웹 페이지나 리소스를 식별하는 데 사용됩니다.
    /// </summary>
    public required string Url { get; set; }
}
