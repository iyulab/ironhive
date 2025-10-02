using IronHive.Abstractions.Catalog;
using IronHive.Providers.Ollama.Clients;

namespace IronHive.Providers.Ollama;

/// <inheritdoc />
public class OllamaModelCatalog : IModelCatalog
{
    private readonly OllamaModelClient _client;

    public OllamaModelCatalog(string baseUrl)
        : this(new OllamaConfig { BaseUrl = baseUrl })
    { }

    public OllamaModelCatalog(OllamaConfig? config = null)
    {
        _client = new OllamaModelClient(config);
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
        return models.Select(m => new GenericModelSpec
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
            ? new GenericModelSpec
            {
                ModelId = modelId,
            }
            : null;
    }
}