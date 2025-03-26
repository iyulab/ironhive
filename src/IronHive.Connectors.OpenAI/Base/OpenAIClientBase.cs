using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Connectors.OpenAI.Base;

internal abstract class OpenAIClientBase
{
    public HttpClient Client { get; init; }
    public JsonSerializerOptions JsonOptions { get; init; }

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

    /// <summary>
    /// Gets the list of OpenAI models.
    /// </summary>
    public async Task<IEnumerable<OpenAIModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await Client.GetFromJsonAsync<JsonDocument>(
            OpenAIConstants.GetModelListPath.RemovePreffix('/'), cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>();

        return models?.OrderByDescending(m => m.Created).ToArray() ?? [];
    }

    /// <summary>
    /// Creates a new instance of <see cref="HttpClient"/> with the specified configuration settings.
    /// </summary>
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
                OpenAIConstants.AuthorizationHeaderName, string.Format(OpenAIConstants.AuthorizationHeaderValue, config.ApiKey));

        if (!string.IsNullOrWhiteSpace(config.Organization))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.OrganizationHeaderName, config.Organization);

        if (!string.IsNullOrWhiteSpace(config.Project))
            client.DefaultRequestHeaders.Add(
                OpenAIConstants.ProjectHeaderName, config.Project);

        return client;
    }
}
