using System.Text.Json.Serialization;

namespace Raggle.Connectors.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextResponseFormat), "text")]
[JsonDerivedType(typeof(JsonObjectResponseFormat), "json_object")]
[JsonDerivedType(typeof(JsonSchemaResponseFormat), "json_schema")]
internal class ResponseFormat { }

internal class TextResponseFormat : ResponseFormat { }

internal class JsonObjectResponseFormat : ResponseFormat { }

internal class JsonSchemaResponseFormat : ResponseFormat
{
    [JsonPropertyName("json_schema")]
    public required JsonSchema JsonSchema { get; set; }
}

internal class JsonSchema
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("schema")]
    public object? Schema { get; set; }

    [JsonPropertyName("strict")]
    public bool? Strict { get; set; }
}
