using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ResponsesFunctionTool), "function")]
[JsonDerivedType(typeof(ResponsesCustomTool), "custom")]
internal class ResponsesTool
{ }

internal class ResponsesFunctionTool : ResponsesTool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("parameters")]
    public required object Parameters { get; set; }

    /// <summary>
    /// 과도하게 엄격함 (기본: false).
    /// </summary>
    [JsonPropertyName("strict")]
    public bool Strict { get; set; } = false;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal class ResponsesCustomTool : ResponsesTool
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("format")]
    public object? Format { get; set; }
}