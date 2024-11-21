namespace Raggle.Server.WebApi.Models;

public class RaggleServiceConfig
{
    public string DbConnectionString { get; set; } = string.Empty;

    public string DocumentStoragePath { get; set; } = string.Empty;

    public string VectorStoragePath { get; set; } = string.Empty;

    public string OpenAIKey { get; set; } = string.Empty;

    public string AnthropicKey { get; set; } = string.Empty;

    public string OllamaEndpoint { get; set; } = string.Empty;
}
