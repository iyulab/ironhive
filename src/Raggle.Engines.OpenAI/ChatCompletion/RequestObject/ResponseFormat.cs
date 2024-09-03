using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion.ChatRequestObject;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextResponseFormat), "text")]
[JsonDerivedType(typeof(JsonObjectResponseFormat), "json_object")]
[JsonDerivedType(typeof(JsonSchemaResponseFormat), "json_schema")]
public class ResponseFormat { }

public class TextResponseFormat : ResponseFormat { }

public class JsonObjectResponseFormat : ResponseFormat { }

public class JsonSchemaResponseFormat : ResponseFormat
{
    [JsonPropertyName("json_schema")]
    public required JsonSchema JsonSchema { get; set; }
}

public class JsonSchema
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
