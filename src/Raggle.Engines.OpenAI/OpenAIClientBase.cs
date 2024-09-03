using System.Net.Http.Json;
using System.Text.Json;
using Raggle.Engines.OpenAI.Configurations;

namespace Raggle.Engines.OpenAI;

public abstract class OpenAIClientBase
{
    protected readonly HttpClient _client;

    protected OpenAIClientBase(string apiKey)
    {
        _client = CreateHttpClient(new OpenAIConfig { ApiKey = apiKey });
    }

    protected OpenAIClientBase(OpenAIConfig config)
    {
        _client = CreateHttpClient(config);
    }

    /// <summary>
    /// Gets the list of OpenAI models.
    /// </summary>
    /// <param name="filters">Optional filters to apply.</param>
    /// <param name="limit">The maximum number of models to return.</param>
    /// <returns>The list of OpenAI models.</returns>
    protected async Task<IEnumerable<OpenAIModel>> GetModelsAsync(
        OpenAIModelType[]? filters = null,
        int limit = -1,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = OpenAIConstants.Host,
            Path = OpenAIConstants.GetModelsPath
        }.ToString();

        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(requestUri, cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>();

        // Apply filters and limit the number of models returned
        models = models?
            .Where(m => filters == null || filters.Length == 0 || filters.Contains(m.Type))
            .GroupBy(m => m.Type)
            .SelectMany(group => group.OrderByDescending(m => m.Created))
            .Take(limit > 0 ? limit : models.Count())
            .ToArray();

        return models ?? [];
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
