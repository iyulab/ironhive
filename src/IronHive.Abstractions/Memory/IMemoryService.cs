namespace IronHive.Abstractions.Memory;

public interface IMemoryService
{
    Task CreateCollectionAsync(
        string collectionName,
        string embedModel,
        CancellationToken cancellationToken = default);

    Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    Task MemorizeFileAsync(
        string collectionName,
        string documentId,
        string fileName,
        string[] steps,
        IDictionary<string, object>? options = null,
        string[]? tags = null,
        CancellationToken cancellationToken = default);

    Task UnMemorizeFileAsync(
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
