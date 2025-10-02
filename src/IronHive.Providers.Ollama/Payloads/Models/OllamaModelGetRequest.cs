using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Payloads.Models;

internal class OllamaModelGetRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("verbose")]
    public bool? Verbose { get; set; }
}
