using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.ChatCompletion;

internal class ToolCall
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("function")]
    public FunctionCall? Function { get; set; }
}

internal class FunctionCall
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public string? Arguments { get; set; }
}
