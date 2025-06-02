using IronHive.Abstractions.Embedding;

namespace IronHive.Core.Services;

public class EmbeddingService : IEmbeddingService
{
    private readonly Dictionary<string, IEmbeddingProvider> _providers;

    public EmbeddingService(IEnumerable<IEmbeddingProvider> providers)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string provider,
        string model,
        string input,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var result = await service.EmbedAsync(model, input, cancellationToken);
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(provider, out var service))
            throw new KeyNotFoundException($"Service key '{provider}' not found.");

        var results = await service.EmbedBatchAsync(model, inputs, cancellationToken);
        return results;
    }
}
