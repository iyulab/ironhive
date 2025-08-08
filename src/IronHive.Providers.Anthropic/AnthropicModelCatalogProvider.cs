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
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSummary>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var req = new AnthropicListModelsRequest
        {
            Limit = 999,
        };
        var res = await _client.GetModelsAsync(req, cancellationToken);

        return res.Data.Select(m => new ModelSummary
        {
            Provider = ProviderName,
            ModelId = m.Id,
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
                ModelId = model.Id,
                DisplayName = model.DisplayName,
                CreatedAt = model.CreatedAt,
            }
            : null;
    }
}
