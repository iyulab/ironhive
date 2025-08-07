using System.Text.RegularExpressions;
using System.Numerics.Tensors;
using LiteDB;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Storages;

/// <summary>
/// LiteDB를 이용한 로컬 벡터 스토리지 구현입니다.
/// </summary>
public partial class LocalVectorStorage : IVectorStorage
{
    private const string CollectionMetaTableName = "collections_meta";
    private static readonly Regex _collPattern = new Regex(@"^[A-Za-z][A-Za-z0-9_]*$", RegexOptions.Compiled);
    private readonly LiteDatabase _db;

    public LocalVectorStorage(LocalVectorConfig config)
    {
        _db = CreateLiteDatabase(config);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<IEnumerable<VectorCollection>> ListCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        var meta = _db.GetCollection<VectorCollection>(CollectionMetaTableName);
        var result = meta.FindAll();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        var meta = _db.GetCollection<VectorCollection>(CollectionMetaTableName);
        var exists = meta.Exists(x => x.Name == collectionName);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(exists);
    }

    /// <inheritdoc />
    public Task<VectorCollection?> GetCollectionInfoAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var meta = _db.GetCollection<VectorCollection>(CollectionMetaTableName);
        var coll = meta.FindOne(x => x.Name == collectionName);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(coll ?? null);
    }

    /// <inheritdoc />
    public Task CreateCollectionAsync(
        VectorCollection collection,
        CancellationToken cancellationToken = default)
    {
        var collectionName = EnsureCollectionName(collection.Name);
        if (_db.CollectionExists(collectionName))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        cancellationToken.ThrowIfCancellationRequested();

        // LiteDB는 컬렉션 생성 기능을 따로 제공하지 않고, 자동으로 생성되므로,
        // 임의의 빈 레코드를 추가하여 컬렉션을 초기화합니다.
        var empty = new VectorRecord
        {
            Id = Guid.NewGuid().ToString(),
            Vectors = new float[collection.Dimensions],
            Source = new TextMemorySource { Value = string.Empty },
        };
        coll.Upsert(empty);
        coll.Delete(empty.Id);

        // 인덱스 생성
        coll.EnsureIndex(p => p.Id);
        coll.EnsureIndex(p => p.Source.Id);

        // 메타 테이블에 등록
        var meta = _db.GetCollection<VectorCollection>(CollectionMetaTableName);
        meta.Upsert(collection);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        cancellationToken.ThrowIfCancellationRequested();

        // 인덱스 삭제
        coll.DropIndex(nameof(VectorRecord.Id));
        coll.DropIndex(nameof(VectorRecord.Source.Id));

        // 컬렉션 삭제
        _db.DropCollection(collectionName);

        // 메타 테이블에서 삭제
        var meta = _db.GetCollection<VectorCollection>(CollectionMetaTableName);
        meta.DeleteMany(p => p.Name == collectionName);

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        cancellationToken.ThrowIfCancellationRequested();

        var query = coll.Query();
        if (filter != null && (filter.SourceIds.Count > 0 || filter.VectorIds.Count > 0))
        {
            query = query.Where(p =>
                filter.SourceIds.Count > 0 && filter.SourceIds.Contains(p.Source.Id) ||
                filter.VectorIds.Count > 0 && filter.VectorIds.Contains(p.Id));
        }

        var results = query
            .OrderByDescending(p => p.LastUpsertedAt)
            .Limit(limit)
            .ToEnumerable();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        cancellationToken.ThrowIfCancellationRequested();

        coll.Upsert(records.Select(r =>
        {
            // Set LastUpdatedAt to current time
            r.LastUpsertedAt = DateTime.UtcNow;
            return r;
        }));

        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        cancellationToken.ThrowIfCancellationRequested();

        if (filter.SourceIds.Count > 0)
            coll.DeleteMany(p => filter.SourceIds.Contains(p.Source.Id));
        if (filter.VectorIds.Count > 0)
            coll.DeleteMany(p => filter.VectorIds.Contains(p.Id));

        cancellationToken.ThrowIfCancellationRequested();
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
        collectionName = EnsureCollectionName(collectionName);
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        cancellationToken.ThrowIfCancellationRequested();

        var query = coll.Query();
        if (filter != null && (filter.SourceIds.Count > 0 || filter.VectorIds.Count > 0))
        {
            query = query.Where(p =>
                filter.SourceIds.Count > 0 && filter.SourceIds.Contains(p.Source.Id) ||
                filter.VectorIds.Count > 0 && filter.VectorIds.Contains(p.Id));
        }
        
        var records = query
            .ToList()
            .Select(p => new ScoredVectorRecord
            {
                VectorId = p.Id,
                Score = TensorPrimitives.CosineSimilarity(vector.ToArray(), p.Vectors.ToArray()),
                Source = p.Source,
                Content = p.Content,
                LastUpdatedAt = p.LastUpsertedAt 
            })
            .Where(p => p.Score >= minScore)
            .OrderByDescending(p => p.Score)
            .Take(limit)
            .AsEnumerable();

        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(records);
    }

    /// <summary>
    /// LiteDB 인스턴스를 생성합니다.
    /// </summary>
    private static LiteDatabase CreateLiteDatabase(LocalVectorConfig config)
    {
        var dir = Path.GetDirectoryName(config.Path);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var connectionString = new ConnectionString
        {
            Filename = config.Path,
            Password = config.Password,
            AutoRebuild = config.AutoRebuild,
            Upgrade = config.Upgrade,
            Connection = config.Shared ? ConnectionType.Shared : ConnectionType.Direct,
            ReadOnly = false,
        };
        var mapper = new BsonMapper();
        mapper.Entity<VectorRecord>()
              .Id(p => p.Id);

        return new LiteDatabase(connectionString, mapper);
    }

    /// <summary>
    /// 컬렉션 이름의 유효성을 검사하고, 소문자로 변환하여 표준화합니다.
    /// </summary>
    private static string EnsureCollectionName(string collectionName)
    {
        // LiteDB Rules: 
        // 1. 영문자, 숫자, 밑줄(_)만 허용됩니다.
        // 2. 이름의 앞 첫글자는 영문자만 허용됩니다.
        // 3. 대소문자는 구분하지 않으므로 소문자로 통일합니다.

        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentNullException(nameof(collectionName));

        if (!_collPattern.IsMatch(collectionName))
            throw new ArgumentException("Invalid collection name. The name must contain only English letters, numbers, and underscores, and must start with an English letter.");

        if (string.Equals(collectionName, CollectionMetaTableName, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"The collection name '{CollectionMetaTableName}' is reserved and cannot be used.");

        return collectionName.ToLowerInvariant();
    }
}
