using LiteDB;
using Raggle.Abstractions.Memory;
using System.Numerics.Tensors;

namespace Raggle.Vector.LiteDB;

public class LiteDBVectorStorage : IVectorStorage
{
    private readonly LiteDatabase _db;

    public LiteDBVectorStorage(LiteDBConfig config)
    {
        _db = CreateLiteDatabase(config);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task<IEnumerable<string>> GetCollectionListAsync(CancellationToken cancellationToken = default)
    {
        var collections = _db.GetCollectionNames();
        return Task.FromResult(collections);
    }

    public Task<bool> ExistCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        var isExist = _db.CollectionExists(collectionName);
        return Task.FromResult(isExist);
    }

    public async Task CreateCollectionAsync(string collectionName, ulong vectorSize, CancellationToken cancellationToken = default)
    {
        if (await ExistCollectionAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        _db.GetCollection(collectionName);
    }

    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        if (!await ExistCollectionAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        _db.DropCollection(collectionName);
    }

    public Task DeleteVectorsAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        var collection = _db.GetCollection<VectorPoint>(collectionName);
        collection.DeleteMany(p => p.DocumentId == documentId);
        return Task.CompletedTask;
    }

    public Task UpsertVectorsAsync(string collectionName, IEnumerable<VectorPoint> points, CancellationToken cancellationToken = default)
    {
        var collection = _db.GetCollection<VectorPoint>(collectionName);
        collection.Upsert(points);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<RankedPoint>> SearchVectorsAsync(string collectionName, float[] input, float minScore = 0, ulong limit = 5, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var collection = _db.GetCollection<VectorPoint>(collectionName);
        var query = collection.Query();

        if (filter != null)
        {
            if (filter.DocumentIds.Count > 0)
                query = query.Where(p => filter.DocumentIds.Contains(p.DocumentId));
            if (filter.Tags.Count > 0)
                query = query.Where(p => p.Tags != null && p.Tags.Any(t => filter.Tags.Contains(t)));
        }

        var results = query
            .ToList()
            .Select(p => new RankedPoint
            {
                DocumentId = p.DocumentId,
                Score = TensorPrimitives.CosineSimilarity(input, p.Vectors),
                Text = p.Text,
            })
            .Where(p => p.Score >= minScore)
            .OrderByDescending(p => p.Score)
            .Take((int)limit);

        return Task.FromResult(results.AsEnumerable());
    }

    #region Private Methods

    private static LiteDatabase CreateLiteDatabase(LiteDBConfig config)
    {
        var connectionString = new ConnectionString
        {
            Connection = ConnectionType.Shared,
            Filename = config.DatabasePath,
            Password = config.Password,
            AutoRebuild = false,
            Upgrade = false,
            ReadOnly = false,
        };
        return new LiteDatabase(connectionString);
    }

    #endregion
}
