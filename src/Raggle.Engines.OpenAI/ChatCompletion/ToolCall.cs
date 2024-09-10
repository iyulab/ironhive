using System.Text.Json.Serialization;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public class ToolCall
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("id")]
    public string? ID { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("function")]
    public FunctionCall? Function { get; set; }
}

public class FunctionCall
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}
