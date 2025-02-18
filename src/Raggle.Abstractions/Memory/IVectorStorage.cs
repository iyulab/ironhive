namespace Raggle.Abstractions.Memory;

public interface IVectorStorage : IDisposable
{
    Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default);

    Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VectorPoint>> FindVectorsAsync(
        string collectionName,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorPoint> points,
        CancellationToken cancellationToken = default);

    Task DeleteVectorsAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ScoredVectorPoint>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> input,
        float minScore = 0.0f,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default);
}
