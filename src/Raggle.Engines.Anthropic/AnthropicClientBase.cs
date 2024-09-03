using Raggle.Engines.Anthropic.Configurations;

namespace Raggle.Engines.Anthropic;

public abstract class AnthropicClientBase
{
    protected readonly HttpClient _client;

    protected AnthropicClientBase(AnthropicConfig config)
    {
        _client = CreateHttpClient(config);
    }

    protected AnthropicClientBase(string apiKey)
    {
        _client = CreateHttpClient(new AnthropicConfig { ApiKey = apiKey });
    }

    private static HttpClient CreateHttpClient(AnthropicConfig config)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
            throw new ArgumentException("API key is required.");
        if (string.IsNullOrEmpty(config.Version))
            throw new ArgumentException("Version is required.");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add(AnthropicConstants.AuthHeaderName, config.ApiKey);
        client.DefaultRequestHeaders.Add(AnthropicConstants.VersionHeaderName, config.Version);
        return client;
    }
}
