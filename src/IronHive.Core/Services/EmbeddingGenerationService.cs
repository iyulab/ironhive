using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Services;

/// <inheritdoc />
public class EmbeddingGenerationService : IEmbeddingGenerationService
{
    private readonly Dictionary<string, IEmbeddingGenerator> _providers;

    public EmbeddingGenerationService(IEnumerable<IEmbeddingGenerator> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.EmbedAsync(modelId, input, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.EmbedBatchAsync(modelId, inputs, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        string provider, 
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.CountTokensAsync(modelId, input, cancellationToken);
        return result;
    }
}
