using LiteDB;
using IronHive.Abstractions.Memory;
using System.Numerics.Tensors;

namespace IronHive.Storages.LiteDB;

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

    public Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = _db.GetCollectionNames();
        return Task.FromResult(collections);
    }

    public Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var isExist = _db.CollectionExists(collectionName);
        return Task.FromResult(isExist);
    }

    public async Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        CancellationToken cancellationToken = default)
    {
        if (await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        var coll = _db.GetCollection<VectorPoint>(collectionName);
        var emptyPoint = new VectorPoint();
        coll.Upsert(emptyPoint);
        coll.Delete(emptyPoint.VectorId);
    }

    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        _db.DropCollection(collectionName);
    }

    public Task<IEnumerable<VectorPoint>> FindVectorsAsync(
        string collectionName,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
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

        var results = query.ToList();
        return Task.FromResult(results.AsEnumerable());
    }

    public Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorPoint> points,
        CancellationToken cancellationToken = default)
    {
        collectionName = $"{collectionName}";
        var collection = _db.GetCollection<VectorPoint>(collectionName);
        collection.Upsert(points);
        return Task.CompletedTask;
    }

    public Task DeleteVectorsAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        var collection = _db.GetCollection<VectorPoint>(collectionName);
        collection.DeleteMany(p => p.DocumentId == documentId);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<ScoredVectorPoint>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> input,
        float minScore = 0,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
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
            .Select(p => new ScoredVectorPoint
            {
                VectorId = p.VectorId,
                Score = TensorPrimitives.CosineSimilarity(input.ToArray(), p.Vectors.ToArray()),
                DocumentId = p.DocumentId,
                Tags = p.Tags?.ToArray(),
                Payload = p.Payload,
            })
            .Where(p => p.Score >= minScore)
            .OrderByDescending(p => p.Score)
            .Take(limit)
            .AsEnumerable();

        return Task.FromResult(results.Any() ? results : []);
    }

    #region Private Methods

    private static LiteDatabase CreateLiteDatabase(LiteDBConfig config)
    {
        var connectionString = new ConnectionString
        {
            Connection = ConnectionType.Shared,
            Filename = config.DatabasePath,
            Password = string.IsNullOrWhiteSpace(config.Password) ? null : config.Password,
            AutoRebuild = false,
            Upgrade = false,
            ReadOnly = false,
        };
        var mapper = new BsonMapper();
        mapper.Entity<VectorPoint>()
              .Id(p => p.VectorId);

        return new LiteDatabase(connectionString, mapper);
    }

    #endregion
}
