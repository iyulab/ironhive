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
    [JsonPropertyName("json_schema")]
    public required JsonFormat JsonSchema { get; set; }

    public class JsonFormat
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
}
