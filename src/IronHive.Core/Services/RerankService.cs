using IronHive.Abstractions.Reranking;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class RerankService : IRerankService
{
    private readonly IReadOnlyDictionary<string, IDocumentReranker> _rerankers;

    internal RerankService(IReadOnlyDictionary<string, IDocumentReranker> rerankers)
    {
        _rerankers = rerankers;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, IDocumentReranker> Rerankers => _rerankers;

    /// <inheritdoc />
    public Task<IEnumerable<RerankResult>> RerankAsync(
        string provider,
        string modelId,
        string query,
        IEnumerable<string> documents,
        int? topN = null,
        CancellationToken cancellationToken = default)
    {
        if (!_rerankers.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        return service.RerankAsync(modelId, query, documents, topN, cancellationToken);
    }
}
