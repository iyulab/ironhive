using System.Text.Json.Serialization;

namespace IronHive.Connectors.Ollama.Models;

internal class ShowModelRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; set; }

    [JsonPropertyName("verbose")]
    public bool? Verbose { get; set; }
}
