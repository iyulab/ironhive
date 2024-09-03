namespace Raggle.Engines.Anthropic.Configurations;

/// <summary>
/// Represents the configuration settings required to connect to the Anthropic API.
/// </summary>
public class AnthropicConfig
{
    /// <summary>
    /// Gets or sets the API key used for authenticating requests to the Anthropic API.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the Anthropic API being used.
    /// This value is typically set to a default version to 2023-06-01 currently.
    /// </summary>
    public string Version { get; set; } = AnthropicConstants.VersionHeaderValue;
}
