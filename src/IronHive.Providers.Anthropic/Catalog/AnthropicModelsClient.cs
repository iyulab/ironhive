using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Providers.Anthropic.Catalog;

public class AnthropicModelsClient : AnthropicClientBase
{
    public AnthropicModelsClient(AnthropicConfig config) : base(config)
    { }

    public AnthropicModelsClient(string apiKey) : base(apiKey)
    { }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    public async Task<IEnumerable<AnthropicModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var path = $"{AnthropicConstants.GetModelsPath}?limit=999".RemovePreffix('/');
        var jsonDoc = await _client.GetFromJsonAsync<JsonDocument>(path, _jsonOptions, cancellationToken);

        var models = jsonDoc?.RootElement.GetProperty("data").Deserialize<IEnumerable<AnthropicModel>>(_jsonOptions);
        return models?.OrderByDescending(m => m.CreatedAt)
            .ToArray() ?? [];
    }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    public async Task<AnthropicModel?> GetModelAsync(string modelId, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AnthropicConstants.GetModelsPath, modelId).RemovePreffix('/');
        var model = await _client.GetFromJsonAsync<AnthropicModel>(path, _jsonOptions, cancellationToken);
        return model;
    }
}
