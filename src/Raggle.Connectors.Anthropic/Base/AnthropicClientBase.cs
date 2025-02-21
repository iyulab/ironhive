using Raggle.Connectors.Anthropic.Configurations;
using System.Net.Http.Json;
using System.Text.Json;

namespace Raggle.Connectors.Anthropic.Base;

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

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    public async Task<IEnumerable<AnthropicModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(AnthropicConstants.GetModelListPath, cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("data").Deserialize<IEnumerable<AnthropicModel>>(_jsonOptions);
        return models?.OrderByDescending(m => m.CreatedAt).ToArray() ?? [];
    }

    private static HttpClient CreateHttpClient(AnthropicConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrWhiteSpace(config.BaseUrl)
                ? new Uri(AnthropicConstants.DefaultBaseUrl.EnsureSuffix('/'))
                : new Uri(config.BaseUrl.EnsureSuffix('/')),
            DefaultRequestHeaders =
            {
                {
                    AnthropicConstants.VersionHeaderName,
                    config.Version.OrDefault(AnthropicConstants.VersionHeaderValue)
                }
            },
        };

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            client.DefaultRequestHeaders.Add(AnthropicConstants.AuthorizationHeaderName, config.ApiKey);

        return client;
    }
}
