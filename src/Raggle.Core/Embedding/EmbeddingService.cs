using Raggle.Abstractions;
using Raggle.Abstractions.Embedding;

namespace Raggle.Core.Embedding;

public class EmbeddingService : IEmbeddingService
{
    private readonly IReadOnlyDictionary<string, IEmbeddingConnector> _connectors;
    private readonly ITextParser<(string, string)> _parser;

    public EmbeddingService(
        IHiveServiceRegistry registry,
        ITextParser<(string, string)> parser)
    {
        _connectors = registry.GetKeyedServices<IEmbeddingConnector>();
        _parser = parser;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingModel>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var models = new List<EmbeddingModel>();
        foreach (var (key, connector) in _connectors)
        {
            var serviceModels = await connector.GetModelsAsync(cancellationToken);

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
        var res = await EmbedBatchAsync(new EmbeddingsRequest
        {
            Model = model,
            Input = [input]
        }, cancellationToken);

        return res.Embeddings?.First().Embedding 
            ?? throw new InvalidOperationException("Embedding is null");
    }

    /// <inheritdoc />
    public async Task<EmbeddingsResponse> EmbedBatchAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var (serviceKey, model) = _parser.Parse(request.Model);
        if (!_connectors.TryGetValue(serviceKey, out var connector))
        {
            throw new KeyNotFoundException($"Service key '{serviceKey}' not found.");
        }

        request.Model = model;
        return await connector.EmbedBatchAsync(request, cancellationToken);
    }
}
