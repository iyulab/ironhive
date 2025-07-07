using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(OpenAIFunctionTool), "function")]
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
