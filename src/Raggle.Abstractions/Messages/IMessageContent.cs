using Raggle.Abstractions.Tools;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Messages;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContent), "text")]
[JsonDerivedType(typeof(ImageContent), "image")]
[JsonDerivedType(typeof(ToolContent), "tool")]
public interface IMessageContent 
{ 
    int? Index { get; set; }
}

/// <summary>
/// 콘텐츠 블록의 기본 클래스입니다.
/// </summary>
public abstract class MessageContentBase : IMessageContent
{
    public int? Index { get; set; }
}

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다.(For User && Assistant Role)
/// </summary>
public class TextContent : MessageContentBase
{
    public string? Text { get; set; }
}

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ImageContent : MessageContentBase
{
    public string? Data { get; set; }
}

/// <summary>
/// 도구 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ToolContent : MessageContentBase
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public FunctionArguments? Arguments { get; set; }

    public FunctionResult? Result { get; set; }
}
