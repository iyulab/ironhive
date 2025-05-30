using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Providers.Anthropic.Catalog;

internal class AnthropicModelsClient : AnthropicClientBase
{
    internal AnthropicModelsClient(AnthropicConfig config) : base(config)
    { }

    internal AnthropicModelsClient(string apiKey) : base(apiKey)
    { }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    internal async Task<IEnumerable<AnthropicModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var path = $"{AnthropicConstants.GetModelsPath}?limit=999".RemovePreffix('/');
        var jsonDoc = await Client.GetFromJsonAsync<JsonDocument>(path, JsonOptions, cancellationToken);

        var models = jsonDoc?.RootElement.GetProperty("data").Deserialize<IEnumerable<AnthropicModel>>(JsonOptions);
        return models?.OrderByDescending(m => m.CreatedAt)
            .ToArray() ?? [];
    }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    internal async Task<AnthropicModel?> GetModelAsync(string modelId, CancellationToken cancellationToken)
    {
        var path = Path.Combine(AnthropicConstants.GetModelsPath, modelId).RemovePreffix('/');
        var model = await Client.GetFromJsonAsync<AnthropicModel>(path, JsonOptions, cancellationToken);
        return model;
    }
}
