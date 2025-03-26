using IronHive.Abstractions.Memory;

namespace IronHive.Core.Memory;

public class MemoryService : IMemoryService
{
    public Task CreateCollectionAsync(string collectionName, string embedModel, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task MemorizeFileAsync(string collectionName, string documentId, string fileName, string[] steps, IDictionary<string, object>? options = null, string[]? tags = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ScoredVectorPoint>> SearchSimilarVectorsAsync(string collectionName, string embedModel, string query, float minScore = 0, int limit = 5, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UnMemorizeFileAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}