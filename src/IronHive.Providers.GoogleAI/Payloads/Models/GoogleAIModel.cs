using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Payloads.Models;

internal class GoogleAIModel
{
    /// <summary>
    /// The resource name of the Model. Format: models/{model}.
    /// Example: models/gemini-1.5-flash-001
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("baseModelId")]
    public string? BaseModelId { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("inputTokenLimit")]
    public int? InputTokenLimit { get; set; }

    [JsonPropertyName("outputTokenLimit")]
    public int? OutputTokenLimit { get; set; }

    /// <summary>
    /// The model's supported generation methods (e.g., "generateContent").
    /// </summary>
    [JsonPropertyName("supportedGenerationMethods")]
    public IEnumerable<string> SupportedGenerationMethods { get; set; } = [];

    [JsonPropertyName("thinking")]
    public bool? Thinking { get; set; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    [JsonPropertyName("maxTemperature")]
    public float? MaxTemperature { get; set; }

    [JsonPropertyName("topP")]
    public float? TopP { get; set; }

    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}