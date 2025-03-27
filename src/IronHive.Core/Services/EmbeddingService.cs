using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly IHiveServiceStore _store;

    public EmbeddingService(IHiveServiceStore store)
    {
        _store = store;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<EmbeddingModel>();
        var connectors = _store.GetServices<IEmbeddingConnector>();

        foreach (var (key, conn) in connectors)
        {
            var providerModels = await conn.GetModelsAsync(cancellationToken);
            models.AddRange(providerModels.Select(m =>
            {
                m.Provider = key;
                return m;
            }));
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<EmbeddingModel> GetModelAsync(
        string provider,
        string model,
        CancellationToken cancellationToken = default)
    {
        var conn = _store.GetService<IEmbeddingConnector>(provider);
        var providerModel = await conn.GetModelAsync(model, cancellationToken);
        providerModel.Provider = provider;
        return providerModel;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string provider,
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        var res = await EmbedBatchAsync(provider, model, [input], cancellationToken);
        var result = res.FirstOrDefault()?.Embedding
            ?? throw new InvalidOperationException("Failed to embedding");

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!_store.TryGetService<IEmbeddingConnector>(provider, out var connector))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        return await connector.EmbedBatchAsync(provider, inputs, cancellationToken);
    }
}
