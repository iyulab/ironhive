using System.Text.Json.Serialization;

namespace IronHive.Connectors.Ollama.Models;

internal class OllamaModelSummary
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("modified_at")]
    public DateTime? ModifiedAt { get; set; }

    [JsonPropertyName("digest")]
    public string? Digest { get; set; }

    [JsonPropertyName("details")]
    public OllamaModelDetail? Details { get; set; }
}