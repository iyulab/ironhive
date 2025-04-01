using Microsoft.Data.Sqlite;
using IronHive.Abstractions.Memory;
using System.Numerics.Tensors;
using System.Text.Json;

namespace IronHive.Core.Storages;

/// <summary>
/// LiteDB와 성능 비교 후 변경 또는 삭제
/// </summary>
public class SQLiteVectorStorage : IVectorStorage
{
    private readonly SqliteConnection _conn;

    public SQLiteVectorStorage(string? databasePath = null)
    {
        databasePath ??= LocalStorageConfig.DefaultVectorStoragePath;
        _conn = CreateConnection(databasePath);
        _conn.Open();
    }

    public void Dispose()
    {
        _conn?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListCollectionsAsync(
        string? prefix = null, 
        CancellationToken cancellationToken = default)
    {
        prefix ??= string.Empty;
        var results = new List<string>();

        var sql = $@"SELECT name FROM sqlite_master WHERE type='table' AND name LIKE '{prefix}%'";
        using (var cmd = _conn.CreateCommand())
        {
            cmd.CommandText = sql;
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var tableName = reader.GetString(0);
                results.Add(tableName);
            }
        }
        return results;
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);

        var sql = $@"SELECT name FROM sqlite_master WHERE type='table' AND name = @tableName";

        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@tableName", collectionName);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result != null;
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

        var sql = $@"
                CREATE TABLE {collectionName} (
                    Id TEXT PRIMARY KEY,
                    SourceId TEXT NOT NULL INDEX,
                    Source TEXT,
                    Vectors TEXT NOT NULL,
                    Content TEXT,
                    LastUpdatedAt TEXT NOT NULL
                );";

        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        var sql = $@"DROP TABLE {collectionName};";
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        var records = new List<VectorRecord>();

        var sql = $@"SELECT Id, SourceId, Source, Vectors, Content, LastUpdatedAt FROM {collectionName}";
        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (filter != null)
        {
            if (filter.SourceIds.Count > 0)
            {
                var paramNames = new List<string>();
                for (int i = 0; i < filter.SourceIds.Count; i++)
                {
                    var paramName = $"@sourceId{i}";
                    paramNames.Add(paramName);
                    parameters.Add(new SqliteParameter(paramName, filter.SourceIds.ElementAt(i)));
                }
                conditions.Add($"SourceId IN ({string.Join(",", paramNames)})");
            }
            if (filter.VectorIds.Count > 0)
            {
                var paramNames = new List<string>();
                for (int i = 0; i < filter.VectorIds.Count; i++)
                {
                    var paramName = $"@vectorId{i}";
                    paramNames.Add(paramName);
                    parameters.Add(new SqliteParameter(paramName, filter.VectorIds.ElementAt(i)));
                }
                conditions.Add($"Id IN ({string.Join(",", paramNames)})");
            }
        }
        if (conditions.Count > 0)
            sql += $@" WHERE " + string.Join(" OR ", conditions);

        sql += $@" ORDER BY LastUpdatedAt DESC LIMIT @limit";
        parameters.Add(new SqliteParameter("@limit", limit));

        using (var cmd = _conn.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters.ToArray());
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var record = new VectorRecord
                {
                    Id = reader.GetString(0),
                    Source = new TextMemorySource
                    {
                        Id = reader.GetString(1),
                        Text = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
                    },
                    Vectors = JsonSerializer.Deserialize<float[]>(reader.GetString(3)) ?? new float[0],
                    Content = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LastUpdatedAt = DateTime.Parse(reader.GetString(5))
                };
                records.Add(record);
            }
        }
        return records;
    }

    /// <inheritdoc />
    public async Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);

        using var transaction = _conn.BeginTransaction();
        foreach (var record in records)
        {
            // 벡터 배열을 JSON 문자열로 직렬화
            var vectorJson = JsonSerializer.Serialize(record.Vectors);
            var sourceJson = JsonSerializer.Serialize(record.Source);
            // LastUpdatedAt은 ISO 8601 형식으로 저장
            var lastUpdated = record.LastUpdatedAt.ToString("o");

            if (record.Id != null)
            {
                // 기존 레코드 업데이트
                var sql = $@"
                            UPDATE {collectionName} 
                            SET SourceId = @sourceId, Source = @source, Vectors = @vectors, Content = @content, LastUpdatedAt = @lastUpdatedAt
                            WHERE Id = @id";

                using var cmd = _conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@id", record.Id);
                cmd.Parameters.AddWithValue("@sourceId", record.Source.Id);
                cmd.Parameters.AddWithValue("@source", sourceJson);
                cmd.Parameters.AddWithValue("@vectors", vectorJson);
                cmd.Parameters.AddWithValue("@content", record.Content ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@lastUpdatedAt", lastUpdated);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            else
            {
                // 새로운 레코드 삽입
                var sql = $@"
                            INSERT INTO {collectionName} (Id, SourceId, SourceText, Vectors, Content, LastUpdatedAt)
                            VALUES (@Id, @sourceId, @sourceText, @vectors, @content, @lastUpdatedAt);
                            SELECT last_insert_rowid();";

                using var cmd = _conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@Id", record.Id);
                cmd.Parameters.AddWithValue("@sourceId", record.Source.Id);
                cmd.Parameters.AddWithValue("@sourceText", JsonSerializer.Serialize(record.Source));
                cmd.Parameters.AddWithValue("@vectors", vectorJson);
                cmd.Parameters.AddWithValue("@content", record.Content ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@lastUpdatedAt", lastUpdated);
                var result = await cmd.ExecuteScalarAsync(cancellationToken);
            }
        }
        transaction.Commit();
    }

    /// <inheritdoc />
    public async Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);

        if (filter.SourceIds.Count > 0)
        {
            var paramNames = new List<string>();
            var parameters = new List<SqliteParameter>();
            for (int i = 0; i < filter.SourceIds.Count; i++)
            {
                var paramName = $"@sourceId{i}";
                paramNames.Add(paramName);
                parameters.Add(new SqliteParameter(paramName, filter.SourceIds.ElementAt(i)));
            }
            var sql = $@"DELETE FROM {collectionName} WHERE SourceId IN ({string.Join(",", paramNames)})";
            
            using var cmd = _conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters.ToArray());
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        if (filter.VectorIds.Count > 0)
        {
            var paramNames = new List<string>();
            var parameters = new List<SqliteParameter>();
            for (int i = 0; i < filter.VectorIds.Count; i++)
            {
                var paramName = $"@vectorId{i}";
                paramNames.Add(paramName);
                parameters.Add(new SqliteParameter(paramName, filter.VectorIds.ElementAt(i)));
            }
            var sql = $@"DELETE FROM {collectionName} WHERE Id IN ({string.Join(",", paramNames)})";

            using var cmd = _conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters.ToArray());
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> vector,
        float minScore = 0,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        var results = new List<ScoredVectorRecord>();

        var sql = $@"SELECT Id, SourceId, SourceText, Vectors, Content, LastUpdatedAt FROM {collectionName}";
        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (filter != null)
        {
            if (filter.SourceIds.Count > 0)
            {
                var paramNames = new List<string>();
                for (int i = 0; i < filter.SourceIds.Count; i++)
                {
                    var paramName = $"@sourceId{i}";
                    paramNames.Add(paramName);
                    parameters.Add(new SqliteParameter(paramName, filter.SourceIds.ElementAt(i)));
                }
                conditions.Add($"SourceId IN ({string.Join(",", paramNames)})");
            }
            if (filter.VectorIds.Count > 0)
            {
                var paramNames = new List<string>();
                for (int i = 0; i < filter.VectorIds.Count; i++)
                {
                    var paramName = $"@vectorId{i}";
                    paramNames.Add(paramName);
                    parameters.Add(new SqliteParameter(paramName, filter.VectorIds.ElementAt(i)));
                }
                conditions.Add($"Id IN ({string.Join(",", paramNames)})");
            }
        }
        if (conditions.Count > 0)
            sql += $@" WHERE " + string.Join(" OR ", conditions);

        using (var cmd = _conn.CreateCommand())
        {
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters.ToArray());
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var source = JsonSerializer.Deserialize<IMemorySource>(reader.GetString(2))
                    ?? throw new Exception("Failed to deserialize source.");
                var vectors = JsonSerializer.Deserialize<float[]>(reader.GetString(3))
                    ?? throw new Exception("Failed to deserialize vectors.");

                var rec = new VectorRecord
                {
                    Id = reader.GetString(0),
                    Source = source,
                    Vectors = vectors,
                    Content = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LastUpdatedAt = DateTime.Parse(reader.GetString(5))
                };

                // 코사인 유사도 계산 (TensorPrimitives.CosineSimilarity 메서드를 그대로 사용)
                var score = TensorPrimitives.CosineSimilarity(vector.ToArray(), rec.Vectors.ToArray());
                if (score >= minScore)
                {
                    results.Add(new ScoredVectorRecord
                    {
                        VectorId = rec.Id,
                        Score = score,
                        Source = rec.Source,
                        Content = rec.Content,
                        LastUpdatedAt = rec.LastUpdatedAt
                    });
                }
            }
        }
        return results.OrderByDescending(r => r.Score).Take(limit);
    }

    // 컬렉션 이름의 유효성을 검사
    private static string EnsureCollectionName(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentNullException(nameof(collectionName));

        return collectionName;
    }

    // SQLite 연결 생성
    private SqliteConnection CreateConnection(string databasePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Password = null,
            Cache = SqliteCacheMode.Shared,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 60,
            Pooling = true,                         // 연결 풀링
            ForeignKeys = false,                    // 외래키 미사용
            RecursiveTriggers = false,              // 트리거 미사용
        };
        return new SqliteConnection(builder.ConnectionString);
    }
}
