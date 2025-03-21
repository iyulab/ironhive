using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Connectors.Anthropic;

/// <summary>
/// Represents the configuration settings required to connect to the Anthropic API.
/// </summary>
public class AnthropicConfig
{
    /// <summary>
    /// Gets or sets the endpoint URL for the OpenAI API.
    /// Default value is "https://api.anthropic.com/v1/".
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the API key used for authenticating requests to the Anthropic API.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the Anthropic API being used.
    /// Default value is "2023-06-01".
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON serialization options used for Anthropic API
    /// </summary>
    [JsonIgnore]
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };
}
