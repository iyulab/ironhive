using System.Text.Json;

namespace IronHive.Providers.OpenAI.Clients;

public abstract class OpenAIClientBase : IDisposable
{
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected OpenAIClientBase(OpenAIConfig config)
    {
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    protected OpenAIClientBase(string apiKey)
    {
        var config = new OpenAIConfig { ApiKey = apiKey };
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    public virtual void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    private static HttpClient CreateHttpClient(OpenAIConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new ArgumentException(
                "OpenAI API key is required. Set it via OpenAIConfig.ApiKey.", nameof(config));

        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrWhiteSpace(config.BaseUrl)
                ? new Uri(OpenAIConstants.DefaultBaseUrl.EnsureSuffix('/'))
                : new Uri(config.BaseUrl.EnsureSuffix('/'))
        };

        client.DefaultRequestHeaders.Add(
            OpenAIConstants.AuthorizationHeaderName, string.Format(OpenAIConstants.AuthorizationHeaderValue, config.ApiKey));

        if (!string.IsNullOrWhiteSpace(config.Organization))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.OrganizationHeaderName, config.Organization);

        if (!string.IsNullOrWhiteSpace(config.Project))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.ProjectHeaderName, config.Project);

        if (config.DefaultHeaders != null)
        {
            foreach (var header in config.DefaultHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
        return client;
    }
}
