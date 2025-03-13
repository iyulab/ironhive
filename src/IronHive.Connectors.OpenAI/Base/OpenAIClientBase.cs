using System.Net.Http.Json;
using System.Text.Json;
using IronHive.Connectors.OpenAI.Configurations;

namespace IronHive.Connectors.OpenAI.Base;

internal abstract class OpenAIClientBase
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

    /// <summary>
    /// Gets the list of OpenAI models.
    /// </summary>
    public async Task<IEnumerable<OpenAIModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(
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
