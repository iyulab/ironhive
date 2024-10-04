using Raggle.Abstractions.Tools;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Models;

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


/// <summary>
/// TODO: Working on it
/// </summary>
public class JsonContentBlockConverter : JsonConverter<IContentBlock>
{
    private readonly string _typeName;
    private readonly IDictionary<string, Type> _mapper;

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(IContentBlock).IsAssignableFrom(typeToConvert);
    }

    public JsonContentBlockConverter(string typeName, IDictionary<string, Type> mapper)
    {
        _typeName = typeName;
        _mapper = mapper;
    }

    public override IContentBlock? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        var root = jsonDocument.RootElement;

        var type = root.GetProperty("Type").GetString();

        return type switch
        {
            "Text" => JsonSerializer.Deserialize<TextContentBlock>(root.GetRawText(), options),
            "File" => JsonSerializer.Deserialize<FileContentBlock>(root.GetRawText(), options),
            "Image" => JsonSerializer.Deserialize<ImageContentBlock>(root.GetRawText(), options),
            "Tool" => JsonSerializer.Deserialize<ToolContentBlock>(root.GetRawText(), options),
            "Memory" => JsonSerializer.Deserialize<MemoryContentBlock>(root.GetRawText(), options),
            "Error" => JsonSerializer.Deserialize<ErrorContentBlock>(root.GetRawText(), options),
            _ => null
        };
    }

    public override void Write(Utf8JsonWriter writer, IContentBlock value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case TextContentBlock textContentBlock:
                JsonSerializer.Serialize(writer, textContentBlock, options);
                break;
            case FileContentBlock fileContentBlock:
                JsonSerializer.Serialize(writer, fileContentBlock, options);
                break;
            case ImageContentBlock imageContentBlock:
                JsonSerializer.Serialize(writer, imageContentBlock, options);
                break;
            case ToolContentBlock toolContentBlock:
                JsonSerializer.Serialize(writer, toolContentBlock, options);
                break;
            case MemoryContentBlock memoryContentBlock:
                JsonSerializer.Serialize(writer, memoryContentBlock, options);
                break;
            case ErrorContentBlock errorContentBlock:
                JsonSerializer.Serialize(writer, errorContentBlock, options);
                break;
        }
    }
}