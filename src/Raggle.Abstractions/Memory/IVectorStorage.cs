namespace Raggle.Abstractions.Memory;

public interface IVectorStorage : IDisposable
{
    Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task CreateCollectionAsync(
        string collectionName,
        ulong vectorSize,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ScoredVectorPoint>> SearchVectorsAsync(
        string collectionName,
        float[] input,
        float minScore = 0.0f,
        ulong limit = 5,
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
}
