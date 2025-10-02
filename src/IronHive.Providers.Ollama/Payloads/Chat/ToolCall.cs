using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Payloads.Chat;

internal class ToolCall
{
    [JsonPropertyName("function")]
    public FunctionCall? Function { get; set; }
}

internal class FunctionCall
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("arguments")]
    public IDictionary<string, object>? Arguments { get; set; }
}
