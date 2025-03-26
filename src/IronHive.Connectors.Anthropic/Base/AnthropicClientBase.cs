using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Connectors.Anthropic.Base;

internal abstract class AnthropicClientBase
{
    public HttpClient Client { get; init; }
    public JsonSerializerOptions JsonOptions { get; init; }

    protected AnthropicClientBase(AnthropicConfig config)
    {
        Client = CreateHttpClient(config);
        JsonOptions = config.JsonOptions;
    }

    protected AnthropicClientBase(string apiKey)
    {
        var config = new AnthropicConfig { ApiKey = apiKey };
        Client = CreateHttpClient(config);
        JsonOptions = config.JsonOptions;
    }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    public async Task<IEnumerable<AnthropicModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await Client.GetFromJsonAsync<JsonDocument>(AnthropicConstants.GetModelListPath.RemovePreffix('/'), cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("data").Deserialize<IEnumerable<AnthropicModel>>(JsonOptions);
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
