using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Catalog;

internal class OllamaModelGetRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("verbose")]
    public bool? Verbose { get; set; }
}
