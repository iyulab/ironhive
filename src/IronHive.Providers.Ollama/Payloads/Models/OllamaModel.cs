using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Payloads.Models;

internal class OllamaModel
{
    [JsonPropertyName("modelfile")]
    public string? ModelFile { get; set; }

    [JsonPropertyName("parameters")]
    public string? Parameters { get; set; }

    [JsonPropertyName("template")]
    public string? Template { get; set; }

    [JsonPropertyName("details")]
    public OllamaModelDetail? Details { get; set; }

    [JsonPropertyName("model_info")]
    public IDictionary<string, object?>? ModelInfo { get; set; }

    [JsonPropertyName("capabilities")]
    public IEnumerable<string>? Capabilities { get; set; }
}
