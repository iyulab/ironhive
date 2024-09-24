using Raggle.Abstractions.Tools;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextContentBlock), "text")]
[JsonDerivedType(typeof(ImageContentBlock), "image")]
[JsonDerivedType(typeof(ToolContentBlock), "tool")]
public abstract class ContentBlock
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }
}

public class TextContentBlock : ContentBlock
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class ImageContentBlock : ContentBlock
{
    [JsonPropertyName("data")]
    public string? Data { get; set; }
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
