using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.Responses;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextResponseFormat), "text")]
[JsonDerivedType(typeof(JsonObjectResponseFormat), "json_object")]
[JsonDerivedType(typeof(JsonSchemaResponseFormat), "json_schema")]
public abstract class ResponsesFormat 
{ }

public class TextResponseFormat : ResponsesFormat { }

public class JsonObjectResponseFormat : ResponsesFormat { }

public class JsonSchemaResponseFormat : ResponsesFormat
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("schema")]
    public object? Schema { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("strict")]
    public bool? Strict { get; set; }
}
