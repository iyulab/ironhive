using System.Text.Json;

namespace IronHive.Providers.Anthropic;

public abstract class AnthropicClientBase : IDisposable
{
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

    public HttpClient Client { get; }

    public JsonSerializerOptions JsonOptions { get; }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
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
