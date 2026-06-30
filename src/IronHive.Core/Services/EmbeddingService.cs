using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class EmbeddingService : IEmbeddingService
{
    private readonly IReadOnlyDictionary<string, IEmbeddingGenerator> _generators;

    internal EmbeddingService(IReadOnlyDictionary<string, IEmbeddingGenerator> generators)
    {
        _generators = generators;
    }

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.EmbedAsync(modelId, input, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.EmbedBatchAsync(modelId, inputs, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.CountTokensAsync(modelId, input, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <inheritdoc />
    public Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!_generators.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = service.CountTokensBatchAsync(modelId, inputs, cancellationToken);
        return result;
    }
}
