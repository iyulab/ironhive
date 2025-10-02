using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Payloads.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChatFunctionTool), "function")]
[JsonDerivedType(typeof(ChatCustomTool), "custom")]
public abstract class ChatTool
{ }

public class ChatFunctionTool : ChatTool
{
    [JsonPropertyName("function")]
    public required FunctionSchema Function { get; set; }

    public class FunctionSchema
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public object? Parameters { get; set; }

        /// <summary>
        /// "true" is not working, "false" is default
        /// </summary>
        [JsonPropertyName("strict")]
        public bool Strict { get; } = false;
    }
}

public class ChatCustomTool : ChatTool
{
    [JsonPropertyName("custom")]
    public required CustomSchema Custom { get; set; }

    public class CustomSchema
    {
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("format")]
        public object? Format { get; set; }
    }
}