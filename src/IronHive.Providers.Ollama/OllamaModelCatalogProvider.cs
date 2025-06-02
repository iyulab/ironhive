using IronHive.Abstractions.Catalog;
using IronHive.Providers.Ollama.Catalog;

namespace IronHive.Providers.Ollama;

public class OllamaModelCatalogProvider : IModelCatalogProvider
{
    private readonly OllamaModelClient _client;

    public OllamaModelCatalogProvider(OllamaConfig? config = null)
    {
        _client = new OllamaModelClient(config);
    }

    public OllamaModelCatalogProvider(string baseUrl)
    {
        _client = new OllamaModelClient(baseUrl);
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
            ModelId = m.Name,
        });
    }

    /// <inheritdoc />
    public async Task<ModelDetails?> FindModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var model = await _client.GetModelAsync(modelId, cancellationToken);
        if (model == null)
        {
            return null;
        }

        return new ModelDetails
        {
            Provider = ProviderName,
            ModelId = modelId,
            Capabilities = model.Capabilities,
        };
    }
}
