using IronHive.Abstractions.Catalog;
using IronHive.Providers.OpenAI.Catalog;

namespace IronHive.Providers.OpenAI;

public class OpenAIModelCatalogProvider : IModelCatalogProvider
{
    private readonly OpenAIModelsClient _client;

    public OpenAIModelCatalogProvider(OpenAIConfig config)
    {
        _client = new OpenAIModelsClient(config);
    }

    public OpenAIModelCatalogProvider(string apiKey)
    {
        _client = new OpenAIModelsClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public async Task<IEnumerable<ModelSummary>> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        var models = await _client.GetListModelsAsync(cancellationToken);

        return models.Select(m => new ModelSummary
        {
            Provider = ProviderName,
            ModelId = m.Id,
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
                OwnedBy = model.OwnedBy,
                CreatedAt = model.Created,
            }
            : null;
    }
}
