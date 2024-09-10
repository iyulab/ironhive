using Raggle.Engines.Anthropic.Configurations;
using System.Text.Json;

namespace Raggle.Engines.Anthropic.Abstractions;

public abstract class AnthropicClientBase
{
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected AnthropicClientBase(AnthropicConfig config)
    {
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    protected AnthropicClientBase(string apiKey)
    {
        var defaultConfig = new AnthropicConfig { ApiKey = apiKey };
        _client = CreateHttpClient(defaultConfig);
        _jsonOptions = defaultConfig.JsonOptions;
    }

    protected IEnumerable<AnthropicModel> GetModels()
    {
        return AnthropicConstants.PredefinedModelIds.Select(id => new AnthropicModel { ModelId = id });
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
