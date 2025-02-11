using Raggle.Driver.Anthropic.Configurations;
using System.Net.Http.Json;
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
            BaseAddress = string.IsNullOrEmpty(config.EndPoint)
                ? new Uri(AnthropicConstants.DefaultEndPoint.EnsureSuffix('/'))
                : new Uri(config.EndPoint.EnsureSuffix('/')),
        };

        client.DefaultRequestHeaders.Add(AnthropicConstants.VersionHeaderName, 
            string.IsNullOrEmpty(config.Version)
            ? AnthropicConstants.VersionHeaderValue
            : config.Version);

        if (!string.IsNullOrEmpty(config.ApiKey))
            client.DefaultRequestHeaders.Add(
                AnthropicConstants.ApiKeyHeaderName, config.ApiKey);

        return client;
    }
}
