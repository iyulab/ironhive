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
        var config = new AnthropicConfig { ApiKey = apiKey };
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    protected IEnumerable<AnthropicModel> GetModels()
    {
        return AnthropicConstants.PredefinedModelIds.Select(id => new AnthropicModel { ModelId = id });
    }

    private static HttpClient CreateHttpClient(AnthropicConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrEmpty(config.EndPoint) 
                ? new Uri(AnthropicConstants.DefaultEndPoint) 
                : new Uri(config.EndPoint.EndsWith('/') ? config.EndPoint : $"{config.EndPoint}/"),
        };

        client.DefaultRequestHeaders.Add(
            AnthropicConstants.VersionHeaderName,
            string.IsNullOrEmpty(config.Version) ? AnthropicConstants.VersionHeaderValue : config.Version);

        if (!string.IsNullOrEmpty(config.ApiKey))
            client.DefaultRequestHeaders.Add(AnthropicConstants.ApiKeyHeaderName, config.ApiKey);

        return client;
    }
}
