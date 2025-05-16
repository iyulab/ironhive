using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UserTextContent), "text")]
[JsonDerivedType(typeof(UserFileContent), "file")]
public interface IUserContent
{
    /// <summary>
    /// 콘텐츠 블록의 순서를 나타냅니다.
    /// </summary>
    int? Index { get; set; }
}

/// <summary>
/// 유저 콘텐츠 블록의 기본 클래스입니다.
/// </summary>
public abstract class UserContentBase : IUserContent
{
    /// <inheritdoc />
    public int? Index { get; set; }
}

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다
/// </summary>
public class UserTextContent : UserContentBase
{
    /// <summary>
    /// 텍스트 내용을 나타냅니다
    /// </summary>
    public string? Value { get; set; }
}

public enum FileDataFormat
{
    /// <summary>
    /// URL 링크입니다
    /// </summary>
    Url,

    /// <summary>
    /// Base64 인코딩된 데이터입니다
    /// </summary>
    Base64,

    /// <summary>
    /// 일반 텍스트입니다
    /// </summary>
    Text
}

/// <summary>
/// 파일의 콘텐츠 블록을 나타냅니다
/// </summary>
public class UserFileContent : UserContentBase
{
    /// <summary>
    /// 파일의 MIME 타입을 나타냅니다
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 데이터의 형식을 나타냅니다
    /// </summary>
    public FileDataFormat? DataFormat { get; set; }

    /// <summary>
    /// 데이터의 타입에 따른 값을 나타냅니다
    /// </summary>
    public string? Data { get; set; }
}
