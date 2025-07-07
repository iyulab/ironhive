using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextChatResponseFormat), "text")]
[JsonDerivedType(typeof(JsonObjectChatResponseFormat), "json_object")]
[JsonDerivedType(typeof(JsonSchemaChatResponseFormat), "json_schema")]
public class ChatResponseFormat { }

public class TextChatResponseFormat : ChatResponseFormat { }

public class JsonObjectChatResponseFormat : ChatResponseFormat { }

public class JsonSchemaChatResponseFormat : ChatResponseFormat
{
    [JsonPropertyName("json_schema")]
    public required JsonSchemaFormat JsonSchema { get; set; }

    public class JsonSchemaFormat
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
