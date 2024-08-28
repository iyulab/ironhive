using Raggle.Tools.ModelSearch.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Raggle.Tools.ModelSearch.Clients;

/// <summary>
/// a search client for interacting with the OpenAI API.
/// </summary>
public class OpenAIClient
{
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAIClient"/> class.
    /// </summary>
    /// <param name="token">
    /// The OpenAI API token. This token is required to authenticate with the OpenAI API.
    /// </param>
    public OpenAIClient(string token)
    {
        _client = CreateHttpClient(token);
    }

    private static readonly KeyValuePair<string, OpenAIModelType> DefaultModelType = new(string.Empty, OpenAIModelType.Unknown);
    private static readonly Dictionary<string, OpenAIModelType> ModelTypeMapper = new()
    {
        { "gpt", OpenAIModelType.GPT },
        { "embedding", OpenAIModelType.Embeddings },
        { "dall-e", OpenAIModelType.Dalle },
        { "whisper", OpenAIModelType.Whisper },
        { "tts", OpenAIModelType.TTS },
        { "babbage", OpenAIModelType.GPTBase },
        { "davinci", OpenAIModelType.GPTBase }
    };

    /// <summary>
    /// Gets the list of OpenAI models.
    /// </summary>
    /// <param name="filters">Optional filters to apply.</param>
    /// <param name="limit">The maximum number of models to return.</param>
    /// <returns>The list of OpenAI models.</returns>
    public async Task<IEnumerable<OpenAIModel>> GetModelsAsync(
        OpenAIModelType[]? filters = null,
        int limit = -1,
        CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = Constants.OPENAI_HOST,
            Path = Constants.OPENAI_GET_MODELS_PATH
        }.Uri;

        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(requestUri, cancellationToken);

        // Deserialize the JSON response into a list of OpenAIModel objects
        var models = jsonDocument?.RootElement.GetProperty("data")
                        .EnumerateArray()
                        .Where(modelJson => modelJson.TryGetProperty("id", out var idProperty) && !string.IsNullOrEmpty(idProperty.GetString()))
                        .Select(modelJson =>
                        {
                            var modelId = modelJson.GetProperty("id").GetString()!;
                            var createdAtUnix = modelJson.GetProperty("created").GetInt64();
                            var createdAt = DateTimeOffset.FromUnixTimeSeconds(createdAtUnix).UtcDateTime;
                            var type = ModelTypeMapper
                                            .FirstOrDefault(kvp => modelId.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase), DefaultModelType)
                                            .Value;

                            return new OpenAIModel
                            {
                                Type = type,
                                ModelId = modelId,
                                CreatedAt = createdAt
                            };
                        })
                        .ToArray();

        // Apply filters and limit the number of models returned
        models = models?
            .Where(m => filters == null || !filters.Any() || filters.Contains(m.Type))
            .GroupBy(m => m.Type)
            .SelectMany(group => group.OrderByDescending(m => m.CreatedAt))
            .Take(limit > 0 ? limit : models.Length)
            .ToArray();

        return models ?? [];
    }

    private static HttpClient CreateHttpClient(string token)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }
}
