using IronHive.Abstractions.Embedding;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;

namespace IronHive.Core.Memory;

/// <inheritdoc />
public class MemoryService : IMemoryService
{
    private readonly IStorageRegistry _storages;
    private readonly IEmbeddingService _embedder;

    public MemoryService(IStorageRegistry storages, IEmbeddingService embedder)
    {
        _storages = storages;
        _embedder = embedder;
    }

    /// <inheritdoc />
    public async Task<IMemoryCollection> GetCollectionAsync(
        string storageName, 
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(storageName, out var storage))
            throw new InvalidOperationException($"Storage '{storageName}' is not registered.");
        var collectionInfo = await storage.GetCollectionInfoAsync(collectionName, cancellationToken)
            ?? throw new InvalidOperationException($"Collection '{collectionName}' does not exist in storage '{storageName}'.");

        return new MemoryCollection(_storages, _embedder)
        {
            StorageName = storageName,
            CollectionName = collectionName,
            EmbeddingProvider = collectionInfo.EmbeddingProvider,
            EmbeddingModel = collectionInfo.EmbeddingModel,
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(
        string storageName,
        string? prefix = null, 
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(storageName, out var storage))
            throw new InvalidOperationException($"Storage '{storageName}' is not registered.");

        return await storage.ListCollectionsAsync(cancellationToken)
            .ContinueWith(t => string.IsNullOrWhiteSpace(prefix) 
                    ? t.Result 
                    : t.Result.Where(c => c.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)), 
                cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string storageName,
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(storageName, out var storage))
            throw new InvalidOperationException($"Storage '{storageName}' is not registered.");

        return await storage.CollectionExistsAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string storageName,
        string collectionName, 
        string embeddingProvider, 
        string embeddingModel, 
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(storageName, out var storage))
            throw new InvalidOperationException($"Storage '{storageName}' is not registered.");

        if (await storage.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        const string sample = "dimension calculation sample";
        var embeddings = await _embedder.EmbedAsync(embeddingProvider, embeddingModel, sample, cancellationToken)
            ?? throw new InvalidOperationException("Failed to get embeddings for dimension calculation.");

        await storage.CreateCollectionAsync(new VectorCollectionInfo
        {
            Name = collectionName,
            Dimensions = embeddings.Count(),
            EmbeddingProvider = embeddingProvider,
            EmbeddingModel = embeddingModel,
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string storageName,
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        if (!_storages.TryGet<IVectorStorage>(storageName, out var storage))
            throw new InvalidOperationException($"Storage '{storageName}' is not registered.");

        await storage.DeleteCollectionAsync(collectionName, cancellationToken);
    }
}
