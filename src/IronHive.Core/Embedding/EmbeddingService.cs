using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Embedding;

public class EmbeddingService : IEmbeddingService
{
    private readonly IReadOnlyDictionary<string, IEmbeddingConnector> _connectors;

    public EmbeddingService(IReadOnlyDictionary<string, IEmbeddingConnector> connectors)
    {
        _connectors = connectors;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<EmbeddingModel>();

        foreach (var kvp in _connectors)
        {
            var providerModels = await kvp.Value.GetModelsAsync(cancellationToken);
            models.AddRange(providerModels.Select(m =>
            {
                m.Provider = kvp.Key;
                return m;
            }));
        }

        return models;
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
        if (!_connectors.TryGetValue(provider, out var connector))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        return await connector.EmbedBatchAsync(provider, inputs, cancellationToken);
    }
}
