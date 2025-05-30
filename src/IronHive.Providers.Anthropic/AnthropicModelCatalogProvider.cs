using IronHive.Abstractions.Catalog;
using IronHive.Providers.Anthropic.Catalog;

namespace IronHive.Providers.Anthropic;

public class AnthropicModelCatalogProvider : IModelCatalogProvider
{
    private readonly AnthropicModelsClient _client;

    public AnthropicModelCatalogProvider(AnthropicConfig config)
    {
        _client = new AnthropicModelsClient(config);
    }

    public AnthropicModelCatalogProvider(string apiKey)
    {
        _client = new AnthropicModelsClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSummary>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetModelsAsync(cancellationToken);

        return models.Select(m => new ModelSummary
        {
            Provider = ProviderName,
            Id = m.Id,
            DisplayName = m.DisplayName,
        });
    }

    /// <inheritdoc />
    public async Task<ModelDetails?> FindModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);

        return model is not null
            ? new ModelDetails
            {
                Provider = ProviderName,
                Id = model.Id,
                DisplayName = model.DisplayName,
                CreatedAt = model.CreatedAt,
            }
            : null;
    }
}
