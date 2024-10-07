using Raggle.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Engines;

/// <summary>
/// 콘텐츠 블록의 기본 인터페이스입니다.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContentBlock), "text")]
[JsonDerivedType(typeof(FileContentBlock), "file")]
[JsonDerivedType(typeof(ImageContentBlock), "image")]
[JsonDerivedType(typeof(ToolContentBlock), "tool")]
[JsonDerivedType(typeof(MemoryContentBlock), "memory")]
[JsonDerivedType(typeof(ErrorContentBlock), "error")]
public interface IContentBlock { }

/// <summary>
/// 텍스트 콘텐츠 블록을 나타냅니다.(For User && Assistant Role)
/// </summary>
public class TextContentBlock : IContentBlock
{
    public string? Text { get; set; }
}

/// <summary>
/// 파일 콘텐츠 블록을 나타냅니다.(For User Role)
/// </summary>
public class FileContentBlock : IContentBlock
{
    public string? FileName { get; set; }

    public string? Extension { get; set; }

    public string? MimeType { get; set; }

    public long? Size { get; set; }

    public string? Data { get; set; }

    public string? Url { get; set; }
}

/// <summary>
/// 이미지 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ImageContentBlock : IContentBlock
{
    public string? Data { get; set; }

    public string? Url { get; set; }
}

/// <summary>
/// 도구(Content) 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ToolContentBlock : IContentBlock
{
    public string? ID { get; set; }

    public string? Name { get; set; }

    public string? Arguments { get; set; }

    public FunctionResult? Result { get; set; }
}

/// <summary>
/// 메모리(Content) 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class MemoryContentBlock : IContentBlock
{
    public string? Index { get; set; }

    public string? FileName { get; set; }

    public string? Partition { get; set; }

    public string? Segment { get; set; }

    public object? MetaData { get; set; }
}

/// <summary>
/// 에러(Content) 콘텐츠 블록을 나타냅니다.(For Assistant Role)
/// </summary>
public class ErrorContentBlock : IContentBlock
{
    public string? Message { get; set; }
}
