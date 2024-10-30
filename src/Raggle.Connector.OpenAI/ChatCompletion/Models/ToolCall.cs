using System.Text.Json.Serialization;

namespace Raggle.Connector.OpenAI.ChatCompletion.Models;

internal class ToolCall
{
    [JsonPropertyName("index")]
    internal int? Index { get; set; }

    [JsonPropertyName("id")]
    internal string? ID { get; set; }

    [JsonPropertyName("type")]
    internal string Type { get; } = "function";

    [JsonPropertyName("function")]
    internal FunctionCall? Function { get; set; }
}

internal class FunctionCall
{
    [JsonPropertyName("name")]
    internal string? Name { get; set; }

    [JsonPropertyName("arguments")]
    internal string? Arguments { get; set; }
}
