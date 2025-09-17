using IronHive.Abstractions.Catalog;
using IronHive.Providers.OpenAI.Catalog;

namespace IronHive.Providers.OpenAI;

public class OpenAIModelCatalog : IModelCatalog
{
    private readonly OpenAIModelsClient _client;

    public OpenAIModelCatalog(OpenAIConfig config)
    {
        _client = new OpenAIModelsClient(config);
    }

    public OpenAIModelCatalog(string apiKey)
    {
        _client = new OpenAIModelsClient(apiKey);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IModelSpec>> ListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = await _client.GetListModelsAsync(cancellationToken);

        return models.Select(m => new GenericModelSpec
        {
            ModelId = m.Id,
            DisplayName = m.Id,
            OwnedBy = m.OwnedBy,
            CreatedAt = m.Created,
        });
    }

    /// <inheritdoc />
    public async Task<IModelSpec?> FindModelAsync(
        string modelId, 
        CancellationToken cancellationToken)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);

        return model is not null
            ? new GenericModelSpec
            {
                ModelId = model.Id,
                DisplayName = model.Id,
                OwnedBy = model.OwnedBy,
                CreatedAt = model.Created,
            }
            : null;
    }
}
