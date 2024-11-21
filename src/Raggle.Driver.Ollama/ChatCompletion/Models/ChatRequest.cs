using Raggle.Driver.Ollama.Base;
using System.Text.Json.Serialization;

namespace Raggle.Driver.Ollama.ChatCompletion.Models;

internal class ChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required ChatMessage[] Messages { get; set; }

    [JsonPropertyName("tools")]
    public Tool[]? Tools { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("options")]
    public ModelOptions? Options { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; }
}
