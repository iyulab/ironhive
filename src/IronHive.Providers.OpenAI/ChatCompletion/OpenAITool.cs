using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(OpenAIFunctionTool), "function")]
[JsonDerivedType(typeof(OpenAICustomTool), "custom")]
public abstract class OpenAITool
{ }

public class OpenAIFunctionTool : OpenAITool
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

public class OpenAICustomTool : OpenAITool
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