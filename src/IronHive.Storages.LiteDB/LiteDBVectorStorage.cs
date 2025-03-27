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

    /// <inheritdoc />
    public Task<IEnumerable<string>> ListCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = _db.GetCollectionNames() 
            ?? Enumerable.Empty<string>();
        return Task.FromResult(collections);
    }

    /// <inheritdoc />
    public Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var isExist = _db.CollectionExists(collectionName);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        int dimensions,
        CancellationToken cancellationToken = default)
    {
        if (await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        var coll = _db.GetCollection<VectorRecord>(collectionName);

        // LiteDB does not support creating an empty collection.
        var empty = new VectorRecord
        { 
            Source = new TextMemorySource { Text = string.Empty },
            Vectors = new float[dimensions]
        };
        coll.Upsert(empty);
        coll.Delete(empty.Id);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        _db.DropCollection(collectionName);
    }

    /// <inheritdoc />
    public Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        var query = coll.Query();

        if (filter != null)
        {
            if (filter.SourceIds.Count > 0)
                query = query.Where(p => filter.SourceIds.Contains(p.Source.Id));
            if (filter.VectorIds.Count > 0)
                query = query.Where(p => filter.VectorIds.Contains(p.Id));
        }

        var results = query
            .OrderByDescending(p => p.LastUpdatedAt)
            .Limit(limit)
            .ToEnumerable();
        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default)
    {
        collectionName = $"{collectionName}";
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        coll.Upsert(records);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default)
    {
        var coll = _db.GetCollection<VectorRecord>(collectionName);

        if (filter.SourceIds.Count > 0)
            coll.DeleteMany(p => filter.SourceIds.Contains(p.Source.Id));
        if (filter.VectorIds.Count > 0)
            coll.DeleteMany(p => filter.VectorIds.Contains(p.Id));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> vector,
        float minScore = 0,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        var query = coll.Query();

        if (filter != null)
        {
            if (filter.SourceIds.Count > 0)
                query = query.Where(p => filter.SourceIds.Contains(p.Source.Id));
            if (filter.VectorIds.Count > 0)
                query = query.Where(p => filter.VectorIds.Contains(p.Id));
        }

        var records = query
            .ToList()
            .Select(p => new ScoredVectorRecord
            {
                VectorId = p.Id,
                Score = TensorPrimitives.CosineSimilarity(vector.ToArray(), p.Vectors.ToArray()),
                Source = p.Source,
                LastUpdatedAt = p.LastUpdatedAt 
            })
            .Where(p => p.Score >= minScore)
            .OrderByDescending(p => p.Score)
            .Take(limit)
            .AsEnumerable();

        return Task.FromResult(records);
    }

    // LiteDB 인스턴스를 생성합니다.
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
        mapper.Entity<VectorRecord>()
              .Id(p => p.Id);

        return new LiteDatabase(connectionString, mapper);
    }
}
