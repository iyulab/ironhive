using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(OpenAIFunctionToolCall), "function")]
public abstract class OpenAIToolCall
{
    [JsonPropertyName("index")]
    public int? Index { get; set; }
}

public class OpenAIFunctionToolCall : OpenAIToolCall
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public FunctionCallSchema? Function { get; set; }

    public class FunctionCallSchema
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }
}
