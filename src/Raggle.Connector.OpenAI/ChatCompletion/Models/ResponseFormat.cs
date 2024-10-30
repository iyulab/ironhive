using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

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
    internal required JsonSchema JsonSchema { get; set; }
}

internal class JsonSchema
{
    [JsonPropertyName("name")]
    internal required string Name { get; set; }

    [JsonPropertyName("description")]
    internal string? Description { get; set; }

    [JsonPropertyName("schema")]
    internal object? Schema { get; set; }

    [JsonPropertyName("strict")]
    internal bool? Strict { get; set; }
}
