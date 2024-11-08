using System.Text.Json.Serialization;

namespace Raggle.Connector.Ollama.Base;

internal class OllamaModel
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

internal class OllamaModelDetail
{
    [JsonPropertyName("parent_model")]
    public string? ParentModel { get; set; }

    [JsonPropertyName("model_id")]
    public string? Format { get; set; }

    [JsonPropertyName("family")]
    public string? Family { get; set; }

    [JsonPropertyName("families")]
    public string[]? Families { get; set; }

    [JsonPropertyName("paramameter_size")]
    public string? ParamameterSize { get; set; }

    [JsonPropertyName("quantization_level")]
    public string? QuantizationLevel { get; set; }
}