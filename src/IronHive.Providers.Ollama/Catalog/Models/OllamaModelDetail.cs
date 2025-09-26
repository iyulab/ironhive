using System.Text.Json.Serialization;

namespace IronHive.Providers.Ollama.Catalog.Models;

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
