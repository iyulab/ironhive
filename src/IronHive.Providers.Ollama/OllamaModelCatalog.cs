using IronHive.Abstractions.Catalog;
using IronHive.Providers.Ollama.Catalog;

namespace IronHive.Providers.Ollama;

public class OllamaModelCatalog : IModelCatalog
{
    private readonly OllamaModelClient _client;

    public OllamaModelCatalog(OllamaConfig? config = null)
    {
        _client = new OllamaModelClient(config);
    }

    public OllamaModelCatalog(string baseUrl)
    {
        _client = new OllamaModelClient(baseUrl);
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
        var models = await _client.GetModelsAsync(cancellationToken);
        return models.Select(m => new ModelSpec
        {
            ModelId = m.Name,
            CreatedAt = m.ModifiedAt,
            UpdatedAt = m.ModifiedAt,
        });
    }

    /// <inheritdoc />
    public async Task<IModelSpec?> FindModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);

        return model is not null
            ? new ModelSpec
            {
                ModelId = modelId,
            }
            : null;
    }
}
