using System.Text.Json.Serialization;

namespace IronHive.Abstractions.ChatCompletion.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContent), "text")]
[JsonDerivedType(typeof(ImageContent), "image")]
[JsonDerivedType(typeof(ToolContent), "tool")]
public interface IMessageContent
{
    string? Id { get; set; }

    int? Index { get; set; }
}

/// <summary>
/// 콘텐츠 블록의 기본 클래스입니다.
/// </summary>
public abstract class MessageContentBase : IMessageContent
{
    public string? Id { get; set; }

    public int? Index { get; set; }
}

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다
/// </summary>
public class TextContent : MessageContentBase
{
    public string? Value { get; set; }
}

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다
/// </summary>
public class ImageContent : MessageContentBase
{
    public string? ContentType { get; set; }

    public string? Data { get; set; }
}

/// <summary>
/// 도구 콘텐츠 블록을 나타냅니다
/// </summary>
public class ToolContent : MessageContentBase
{
    public ToolStatus Status { get; set; } = ToolStatus.Pending;

    public string? Name { get; set; }

    public string? Arguments { get; set; }

    public string? Result { get; set; }
}

/// <summary>
/// Tool Status
/// </summary>
public enum ToolStatus
{
    /// <summary>
    /// 도구가 대기 중인 상태
    /// </summary>
    Pending,

    /// <summary>
    /// 도구가 실행 중인 상태
    /// </summary>
    Running,

    /// <summary>
    /// 도구가 완료된 상태
    /// </summary>
    Completed,

    /// <summary>
    /// 도구가 실패한 상태
    /// </summary>
    Failed
}
