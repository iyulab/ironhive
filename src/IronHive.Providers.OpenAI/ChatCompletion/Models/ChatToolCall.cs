using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChatFunctionToolCall), "function")]
[JsonDerivedType(typeof(ChatCustomToolCall), "custom")]
public abstract class ChatToolCall
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class ChatFunctionToolCall : ChatToolCall
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

public class ChatCustomToolCall : ChatToolCall
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

