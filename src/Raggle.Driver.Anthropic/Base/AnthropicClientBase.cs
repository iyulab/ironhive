using Raggle.Driver.Anthropic.Configurations;
using System.Text.Json;

namespace Raggle.Driver.Anthropic.Base;

internal abstract class AnthropicClientBase
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

        if (string.IsNullOrEmpty(config.Version))
            client.DefaultRequestHeaders.Add(AnthropicConstants.VersionHeaderName, AnthropicConstants.VersionHeaderValue);
        else
            client.DefaultRequestHeaders.Add(AnthropicConstants.VersionHeaderName, config.Version);

        if (!string.IsNullOrEmpty(config.ApiKey))
            client.DefaultRequestHeaders.Add(AnthropicConstants.ApiKeyHeaderName, config.ApiKey);

        return client;
    }
}
