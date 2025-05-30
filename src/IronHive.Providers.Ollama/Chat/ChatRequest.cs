using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Chat;

internal class ChatRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("messages")]
    public required IEnumerable<Message> Messages { get; set; }

    [JsonPropertyName("tools")]
    public IEnumerable<Tool>? Tools { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("options")]
    public OllamaModelOptions? Options { get; set; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; set; }

    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; set; }
}
