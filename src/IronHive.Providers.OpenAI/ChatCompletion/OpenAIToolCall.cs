using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(OpenAIFunctionToolCall), "function")]
[JsonDerivedType(typeof(OpenAICustomToolCall), "custom")]
public abstract class OpenAIToolCall
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class OpenAIFunctionToolCall : OpenAIToolCall
{
    [JsonPropertyName("function")]
    public FunctionSchema? Function { get; set; }

    public class FunctionSchema
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}

public class OpenAICustomToolCall : OpenAIToolCall
{
    [JsonPropertyName("custom")]
    public CustomSchema? Custom { get; set; }

    public class CustomSchema
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("input")]
        public string? Input { get; set; }
    }
}

