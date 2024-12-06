using Raggle.Abstractions.Tools;
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
/// 텍스트 콘텐츠 블록을 나타냅니다.(For User && Assistant Role)
/// </summary>
public class TextContent : IMessageContent
{
    public int? Index { get; set; }

    public string? Text { get; set; }
}

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ImageContent : IMessageContent
{
    public int? Index { get; set; }

    public string? Data { get; set; }
}

/// <summary>
/// 도구(Content) 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ToolContent : IMessageContent
{
    public int? Index { get; set; }

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Arguments { get; set; }

    public FunctionResult? Result { get; set; }
}
