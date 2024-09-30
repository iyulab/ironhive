using Raggle.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContentBlock), "text")]
[JsonDerivedType(typeof(ImageContentBlock), "image")]
[JsonDerivedType(typeof(ToolContentBlock), "tool")]
[JsonDerivedType(typeof(FileContentBlock), "file")]
[JsonDerivedType(typeof(DocumentContentBlock), "document")]
public abstract class ContentBlock { }

public class TextContentBlock : ContentBlock
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class ImageContentBlock : ContentBlock
{
    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class ToolContentBlock : ContentBlock
{
    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public object? Arguments { get; set; }

    [JsonPropertyName("result")]
    public FunctionResult? Result { get; set; }
}

public class FileContentBlock : ContentBlock
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}


public class DocumentContentBlock : ContentBlock
{
    [JsonPropertyName("id")]
    public string? Index { get; set; }

    [JsonPropertyName("mimeType")]
    public string? MimeType { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("partition")]
    public string? Partition { get; set; }

    [JsonPropertyName("segment")]
    public string? Segment { get; set; }
}
