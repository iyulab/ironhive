namespace Raggle.Abstractions.Memory;

public interface IVectorStorage
{
    Task CreateCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<VectorRecord>> FindRecordsAsync(
        string collection,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task UpsertRecordAsync(
        string collection,
        VectorRecord records,
        CancellationToken cancellationToken = default);

    Task UpsertRecordsAsync(
        string collection,
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default);

    Task DeleteRecordsAsync(
        string collection,
        string documentId,
        CancellationToken cancellationToken = default);

    Task SearchRecordsAsync(
        string collection,
        float[] input,
        int limit = 5,
        CancellationToken cancellationToken = default);
}
