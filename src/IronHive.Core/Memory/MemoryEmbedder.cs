using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryEmbedder : IMemoryEmbedder
{
    private readonly IEmbeddingGenerationService _service;

    public MemoryEmbedder(IEmbeddingGenerationService service)
    {
        _service = service;
    }

    /// <inheritdoc />
    public required string Provider { get; init; }

    /// <inheritdoc />
    public required string Model { get; init; }
     
    /// <inheritdoc />
    public Task<int> CountTokensAsync(
        string input, 
        CancellationToken cancellationToken = default)
    {
        return _service.CountTokensAsync(Provider, Model, input, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IEnumerable<float>> EmbedAsync(
        string input, 
        CancellationToken cancellationToken = default)
    {
        return _service.EmbedAsync(Provider, Model, input, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        IEnumerable<string> inputs, 
        CancellationToken cancellationToken = default)
    {
        return _service.EmbedBatchAsync(Provider, Model, inputs, cancellationToken);
    }
}
