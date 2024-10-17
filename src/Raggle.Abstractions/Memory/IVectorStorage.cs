namespace Raggle.Abstractions.Memory;

public interface IVectorStorage
{
    Task CreateCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collection,
        CancellationToken cancellationToken = default);

    Task FindRecordsAsync(
        string collection,
        CancellationToken cancellationToken = default);

    Task UpsertRecordsAsync(
        string collection,
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default);

    Task DeleteRecordsAsync(
        string collection,
        string documentId,
        CancellationToken cancellationToken = default);
}
