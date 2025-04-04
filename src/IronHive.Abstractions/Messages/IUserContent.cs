using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UserTextContent), "text")]
[JsonDerivedType(typeof(UserImageContent), "image")]
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

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다
/// </summary>
public class UserImageContent : UserContentBase
{
    /// <summary>
    /// 이미지의 MIME 타입을 나타냅니다
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 이미지의 데이터 URL or Base64 인코딩된 문자열을 나타냅니다
    /// </summary>
    public string? Data { get; set; }
}
