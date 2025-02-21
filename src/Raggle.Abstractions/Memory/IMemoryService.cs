namespace Raggle.Abstractions.Memory;

public interface IMemoryService
{
    Task CreateCollectionAsync(
        string collectionName,
        string embedModel,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task UploadDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        Stream data,
        CancellationToken cancellationToken = default);

    Task DeleteDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default);

    Task MemorizeDocumentAsync(
        string collectionName,
        string documentId,
        string fileName,
        string[] steps,
        IDictionary<string, object>? options = null,
        string[]? tags = null,
        CancellationToken cancellationToken = default);

    Task UnMemorizeDocumentAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ScoredVectorPoint>> SearchSimilarVectorsAsync(
        string collectionName,
        string embedModel,
        string query,
        float minScore = 0,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default);
}
