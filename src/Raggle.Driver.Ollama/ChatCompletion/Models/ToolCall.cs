using System.Text.Json.Serialization;

namespace Raggle.Driver.Ollama.ChatCompletion.Models;

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
