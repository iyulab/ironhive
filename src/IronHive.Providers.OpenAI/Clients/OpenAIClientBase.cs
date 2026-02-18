using System.Text.Json;

namespace IronHive.Providers.OpenAI.Clients;

public abstract class OpenAIClientBase : IDisposable
{
    protected HttpClient Client { get; }
    protected JsonSerializerOptions JsonOptions { get; }

    protected OpenAIClientBase(OpenAIConfig config)
    {
        Client = CreateHttpClient(config);
        JsonOptions = config.JsonOptions;
    }

    protected OpenAIClientBase(string apiKey)
    {
        var config = new OpenAIConfig { ApiKey = apiKey };
        Client = CreateHttpClient(config);
        JsonOptions = config.JsonOptions;
    }

    public virtual void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    private static HttpClient CreateHttpClient(OpenAIConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrWhiteSpace(config.BaseUrl)
                ? new Uri(OpenAIConstants.DefaultBaseUrl.EnsureSuffix('/'))
                : new Uri(config.BaseUrl.EnsureSuffix('/'))
        };

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.AuthorizationHeaderName, $"Bearer {config.ApiKey}");

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
