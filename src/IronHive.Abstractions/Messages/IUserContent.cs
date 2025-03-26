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
    string? Id { get; set; }

    int? Index { get; set; }
}

/// <summary>
/// 유저 콘텐츠 블록의 기본 클래스입니다.
/// </summary>
public abstract class UserContentBase : IUserContent
{
    public string? Id { get; set; }

    public int? Index { get; set; }
}

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다
/// </summary>
public class UserTextContent : UserContentBase
{
    public string? Value { get; set; }
}

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다
/// </summary>
public class UserImageContent : UserContentBase
{
    public string? ContentType { get; set; }

    public string? Data { get; set; }
}
