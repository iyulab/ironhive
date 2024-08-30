namespace Raggle.Engines.OpenAI;

/// <summary>
/// Represents the configuration settings required to connect to the OpenAI API.
/// </summary>
public class OpenAIConfig
{
    /// <summary>
    /// Gets or sets the API key used for authenticating requests to the OpenAI API.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the organization ID associated with the OpenAI account.
    /// This is optional and may be used to specify which organization to bill.
    /// </summary>
    public string Organization { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project name or identifier that is associated with the OpenAI usage.
    /// This can be used to keep track of requests related to a specific project.
    /// </summary>
    public string Project { get; set; } = string.Empty;
}
