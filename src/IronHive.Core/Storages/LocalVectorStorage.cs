﻿using System.Text.RegularExpressions;
using System.Numerics.Tensors;
using LiteDB;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Storages;

namespace IronHive.Core.Storages;

public partial class LocalVectorStorage : IVectorStorage
{
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
    public Task<IEnumerable<string>> ListCollectionsAsync(
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        prefix ??= string.Empty;
        var colls = _db.GetCollectionNames()
            .Where(p => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            ?? Enumerable.Empty<string>();
        return Task.FromResult(colls);
    }

    /// <inheritdoc />
    public Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);

        var isExist = _db.CollectionExists(collectionName);
        return Task.FromResult(isExist);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        int dimensions,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);

        if (await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        var coll = _db.GetCollection<VectorRecord>(collectionName);

        // 인덱스 생성
        coll.EnsureIndex(p => p.Id);
        coll.EnsureIndex(p => p.SourceId);

        // LiteDB does not support creating an empty collection.
        var empty = new VectorRecord
        {
            Id = string.Empty,
            SourceId = string.Empty,
            Source = new TextMemorySource { Value = string.Empty },
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
        collectionName = EnsureCollectionName(collectionName);

        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        // 인덱스 삭제
        var coll = _db.GetCollection<VectorRecord>(collectionName);
        coll.DropIndex(nameof(VectorRecord.Id));
        coll.DropIndex(nameof(VectorRecord.SourceId));

        // 컬렉션 삭제
        _db.DropCollection(collectionName);
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
        var query = coll.Query();

        if (filter != null && (filter.SourceIds.Count > 0 || filter.VectorIds.Count > 0))
        {
            query = query.Where(p =>
                filter.SourceIds.Count > 0 && filter.SourceIds.Contains(p.SourceId) ||
                filter.VectorIds.Count > 0 && filter.VectorIds.Contains(p.Id));
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
        collectionName = EnsureCollectionName(collectionName);

        records = records.Select(p =>
        {
            p.LastUpdatedAt = DateTime.UtcNow;
            return p;
        });
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
        collectionName = EnsureCollectionName(collectionName);

        var coll = _db.GetCollection<VectorRecord>(collectionName);

        if (filter.SourceIds.Count > 0)
            coll.DeleteMany(p => filter.SourceIds.Contains(p.SourceId));
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
        collectionName = EnsureCollectionName(collectionName);

        var coll = _db.GetCollection<VectorRecord>(collectionName);
        var query = coll.Query();

        if (filter != null && (filter.SourceIds.Count > 0 || filter.VectorIds.Count > 0))
        {
            query = query.Where(p =>
                filter.SourceIds.Count > 0 && filter.SourceIds.Contains(p.SourceId) ||
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
                LastUpdatedAt = p.LastUpdatedAt 
            })
            .Where(p => p.Score >= minScore)
            .OrderByDescending(p => p.Score)
            .Take(limit)
            .AsEnumerable();

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
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentNullException(nameof(collectionName));

        // 1.영문자, 숫자, 밑줄(_)만 허용됩니다.
        // 2. 이름의 앞 첫글자는 영문자만 허용됩니다.
        if (!_collPattern.IsMatch(collectionName))
            throw new ArgumentException("Invalid collection name. The name must contain only English letters, numbers, and underscores, and must start with an English letter.");

        // 3. 대소문자는 구분하지 않으므로 소문자로 통일합니다.
        collectionName = collectionName.ToLowerInvariant();
        return collectionName;
    }
}
