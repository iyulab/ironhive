using System.Text.Json.Serialization;

namespace IronHive.Providers.GoogleAI.Catalog.Models;

/// <summary>
/// A class to hold information about a Generative Language Model.
/// This structure is based on the provided 'Resource: Model' specification.
/// </summary>
public class GoogleAIModel
{
    /// <summary>
    /// The resource name of the Model. Format: models/{model}.
    /// Example: models/gemini-1.5-flash-001
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// The name of the base model. Pass this to the generation request.
    /// Example: gemini-1.5-flash
    /// </summary>
    [JsonPropertyName("baseModelId")]
    public string? BaseModelId { get; set; }

    /// <summary>
    /// The version number of the model. This represents the major version (1.0 or 1.5).
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// The human-readable name of the model. E.g. "Gemini 1.5 Flash".
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// A short description of the model.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Maximum number of input tokens allowed for this model.
    /// </summary>
    [JsonPropertyName("inputTokenLimit")]
    public int? InputTokenLimit { get; set; }

    /// <summary>
    /// Maximum number of output tokens available for this model.
    /// </summary>
    [JsonPropertyName("outputTokenLimit")]
    public int? OutputTokenLimit { get; set; }

    /// <summary>
    /// The model's supported generation methods (e.g., "generateContent").
    /// </summary>
    [JsonPropertyName("supportedGenerationMethods")]
    public IEnumerable<string> SupportedGenerationMethods { get; set; } = [];

    /// <summary>
    /// Whether the model supports thinking.
    /// </summary>
    [JsonPropertyName("thinking")]
    public bool? Thinking { get; set; }

    /// <summary>
    /// Controls the randomness of the output. Default temperature value.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// The maximum temperature this model can use (max value for 'Temperature').
    /// </summary>
    [JsonPropertyName("maxTemperature")]
    public float? MaxTemperature { get; set; }

    /// <summary>
    /// Default value for Nucleus sampling (topP).
    /// </summary>
    [JsonPropertyName("topP")]
    public float? TopP { get; set; }

    /// <summary>
    /// Default value for Top-k sampling (topK). 
    /// If zero, indicates the model doesn't use top-k sampling.
    /// </summary>
    [JsonPropertyName("topK")]
    public int? TopK { get; set; }
}