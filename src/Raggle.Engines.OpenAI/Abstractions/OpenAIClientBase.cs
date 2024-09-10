using System.Net.Http.Json;
using System.Text.Json;
using Raggle.Engines.OpenAI.Configurations;

namespace Raggle.Engines.OpenAI.Abstractions;

public abstract class OpenAIClientBase
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
        var defaultOptions = new OpenAIConfig { ApiKey = apiKey };
        _client = CreateHttpClient(defaultOptions);
        _jsonOptions = defaultOptions.JsonOptions;
    }

    /// <summary>
    /// Gets the list of OpenAI models.
    /// </summary>
    protected async Task<IEnumerable<OpenAIModel>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = OpenAIConstants.Host,
            Path = OpenAIConstants.GetModelsPath
        }.ToString();

        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(requestUri, cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>();

        return models?.OrderByDescending(m => m.Created).ToArray() ?? [];
    }

    private static HttpClient CreateHttpClient(OpenAIConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new ArgumentException("OpenAI API key is required.");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add(OpenAIConstants.AuthHeaderName,
            string.Format(OpenAIConstants.AuthHeaderValue, config.ApiKey));
        if (!string.IsNullOrWhiteSpace(config.Organization))
            client.DefaultRequestHeaders.Add(OpenAIConstants.OrgHeaderName, config.Organization);
        if (!string.IsNullOrWhiteSpace(config.Project))
            client.DefaultRequestHeaders.Add(OpenAIConstants.ProjectHeaderName, config.Project);
        return client;
    }
}
