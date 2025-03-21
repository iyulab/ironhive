using IronHive.Abstractions;
using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Embedding;

public class EmbeddingService : IEmbeddingService
{
    private readonly IReadOnlyDictionary<string, IEmbeddingConnector> _connectors;
    private readonly IServiceModelParser _parser;

    public EmbeddingService(
        IReadOnlyDictionary<string, IEmbeddingConnector> connectors,
        IServiceModelParser parser)
    {
        _connectors = connectors;
        _parser = parser;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<EmbeddingModel>();
        foreach (var (key, con) in _connectors)
        {
            var serviceModels = await con.GetModelsAsync(cancellationToken);

            models.AddRange(serviceModels.Select(x => new EmbeddingModel
            {
                Model = _parser.Stringify((key, x.Model)),
            }));
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        var res = await EmbedBatchAsync(model, [input], cancellationToken);
        var result = res.FirstOrDefault()?.Embedding
            ?? throw new InvalidOperationException("Failed to embedding");

        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var (k, v) = _parser.Parse(model);
        if (!_connectors.TryGetValue(k, out var connector))
            throw new KeyNotFoundException($"Service key '{k}' not found.");

        return await connector.EmbedBatchAsync(v, inputs, cancellationToken);
    }
}
