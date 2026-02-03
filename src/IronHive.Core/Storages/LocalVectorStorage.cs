using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using IronHive.Abstractions.Vector;
using IronHive.Core.Utilities;

namespace IronHive.Core.Storages;

/// <summary>
/// <para> sqlite-vec(vec0) 기반 IVectorStorage 구현체 </para>
/// <see href="https://alexgarcia.xyz/sqlite-vec/features/vec0.html"/>
/// </summary>
public sealed partial class LocalVectorStorage : IVectorStorage
{
    private const string CollectionMetaTable = "vec_collections";

    private readonly string _connectionString;
    private readonly string _moduleVersion;

    private volatile bool _ensuredCollMetaTable = false;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public LocalVectorStorage(LocalVectorConfig config)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = config.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = true,
            DefaultTimeout = 30,
            ForeignKeys = true,
            RecursiveTriggers = false,
        }.ConnectionString;
        _moduleVersion = config.Version;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _initLock.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureCollectionMetaTableAsync();

        var sql = $@"
        SELECT name, dimensions, embedding_provider, embedding_model
        FROM {CollectionMetaTable}
        ORDER BY name";

        using var conn = await CreateConnectionAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);

        var list = new List<VectorCollectionInfo>();
        while (await rdr.ReadAsync(cancellationToken))
        {
            list.Add(new VectorCollectionInfo
            {
                Name = rdr.GetString(0),
                Dimensions = rdr.GetInt64(1),
                EmbeddingProvider = rdr.GetString(2),
                EmbeddingModel = rdr.GetString(3)
            });
        }
        return list;
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        await EnsureCollectionMetaTableAsync();

        var sql = $@"SELECT COUNT(1) FROM {CollectionMetaTable} WHERE name=$n";

        using var conn = await CreateConnectionAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$n", collectionName);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result) > 0;
    }

    /// <inheritdoc />
    public async Task<VectorCollectionInfo?> GetCollectionInfoAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        await EnsureCollectionMetaTableAsync();

        var sql = $@"
        SELECT name, dimensions, embedding_provider, embedding_model
        FROM {CollectionMetaTable} 
        WHERE name=$n";

        using var conn = await CreateConnectionAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$n", collectionName);
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
        if (await rdr.ReadAsync(cancellationToken))
        {
            return new VectorCollectionInfo
            {
                Name = rdr.GetString(0),
                Dimensions = rdr.GetInt64(1),
                EmbeddingProvider = rdr.GetString(2),
                EmbeddingModel = rdr.GetString(3)
            };
        }
        return null;
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        VectorCollectionInfo collection, 
        CancellationToken cancellationToken = default)
    {
        collection.Name = EnsureCollectionName(collection.Name);
        await EnsureCollectionMetaTableAsync();

        using var conn = await CreateConnectionAsync();
        using var tx = await conn.BeginTransactionAsync(cancellationToken);

        // 컬렉션 메타데이터 등록
        {
            var sql = $@"
            INSERT INTO {CollectionMetaTable}(name, dimensions, embedding_provider, embedding_model)
            VALUES ($n, $d, $p, $m)";

            using var insert = conn.CreateCommand();
            insert.CommandText = sql;
            insert.Parameters.AddWithValue("$n", collection.Name);
            insert.Parameters.AddWithValue("$d", collection.Dimensions);
            insert.Parameters.AddWithValue("$p", collection.EmbeddingProvider);
            insert.Parameters.AddWithValue("$m", collection.EmbeddingModel);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        // 컬렉션별 메타 테이블
        {
            var metaTable = VecMetaTable(collection.Name);
            var sql = $@"
            CREATE TABLE IF NOT EXISTS {metaTable}(
                int_id INTEGER PRIMARY KEY AUTOINCREMENT,
                vector_id TEXT NOT NULL UNIQUE,
                source_id TEXT NOT NULL,
                payload BLOB NULL,
                last_upserted_at TEXT NOT NULL
            )";

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = sql;
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }

            // 인덱스
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $@"CREATE INDEX IF NOT EXISTS idx_{metaTable}_source ON {metaTable}(source_id)";
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $@"CREATE INDEX IF NOT EXISTS idx_{metaTable}_last ON {metaTable}(last_upserted_at DESC)";
                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        // 컬렉션별 vec0 테이블 (차원 지정)
        {
            var vecTable = VecTable(collection.Name);
            var sql = $@"
            CREATE VIRTUAL TABLE IF NOT EXISTS {vecTable} USING vec0(
                embedding float[{collection.Dimensions}] distance_metric=cosine
            )";

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        collectionName = EnsureCollectionName(collectionName);
        await EnsureCollectionMetaTableAsync();

        using var conn = await CreateConnectionAsync();
        using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var metaTable = VecMetaTable(collectionName);
        var vecTable = VecTable(collectionName);

        async Task DropIfExists(string type, string name)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DROP {type} IF EXISTS {name}";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // 컬렉션별 테이블 삭제
        await DropIfExists("TABLE", metaTable);
        //await DropIfExists("TABLE", $"sqlite_sequence");
        await DropIfExists("INDEX", $"idx_{metaTable}_source"); // 인덱스 드랍은 자동으로 처리되나 안전차원에서
        await DropIfExists("INDEX", $"idx_{metaTable}_last");
        await DropIfExists("TABLE", vecTable);

        // 컬렉션 메타 삭제
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE FROM {CollectionMetaTable} WHERE name=$n";
            cmd.Parameters.AddWithValue("$n", collectionName);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        using var conn = await CreateConnectionAsync();
        var metaTable = VecMetaTable(collectionName);

        var where = new List<string>();
        var args = new List<SqliteParameter>();

        if (filter != null && filter.VectorIds.Count > 0)
        {
            where.Add($"vector_id IN ({JoinArrayParams("$vid", filter.VectorIds.Count)})");
            args.AddRange(filter.VectorIds.Select((v, i) => new SqliteParameter($"$vid{i}", v)));
        }
        if (filter != null && filter.SourceIds.Count > 0)
        {
            where.Add($"source_id IN ({JoinArrayParams("$sid", filter.SourceIds.Count)})");
            args.AddRange(filter.SourceIds.Select((v, i) => new SqliteParameter($"$sid{i}", v)));
        }

        var sql = $@"
        SELECT vector_id, source_id, payload, last_upserted_at
        FROM {metaTable}
        {(where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "")}
        ORDER BY datetime(last_upserted_at) DESC
        LIMIT $lim";

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddRange(args);
        cmd.Parameters.AddWithValue("$lim", limit);

        var records = new List<VectorRecord>();
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await rdr.ReadAsync(cancellationToken))
        {
            records.Add(new VectorRecord
            {
                VectorId = rdr.GetString(0),
                SourceId = rdr.GetString(1),
                Payload = JsonSerializer.Deserialize<IDictionary<string, object?>>(
                    rdr.IsDBNull(2) ? null : (byte[])rdr["payload"])
                    ?? new Dictionary<string, object?>(),
                LastUpsertedAt = DateTimeOffset.Parse(rdr.GetString(3)).UtcDateTime,
                Vectors = Array.Empty<float>() // 메타 조회이므로 임베딩은 비워둠
            });
        }
        return records;
    }

    /// <inheritdoc />
    public async Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> vectors,
        CancellationToken cancellationToken = default)
    {

        using var conn = await CreateConnectionAsync();
        var vecTable = VecTable(collectionName);
        var metaTable = VecMetaTable(collectionName);

        using var tx = await conn.BeginTransactionAsync(cancellationToken);

        foreach (var v in vectors)
        {
            if (v.Vectors is null) throw new ArgumentException("Vectors가 null입니다.", nameof(vectors));

            // meta upsert
            long intId;
            {
                // 먼저 존재여부 확인
                long? existing;
                using (var sel = conn.CreateCommand())
                {
                    sel.CommandText = $@"SELECT int_id FROM {metaTable} WHERE vector_id=$vid";
                    sel.Parameters.AddWithValue("$vid", v.VectorId);
                    existing = await sel.ExecuteScalarAsync(cancellationToken) as long?;
                }

                if (existing is long l)
                {
                    intId = l;

                    using (var upd = conn.CreateCommand())
                    {
                        upd.CommandText = $@"
                            UPDATE {metaTable}
                            SET source_id=$sid, payload=$p, last_upserted_at=$ts
                            WHERE int_id=$iid";
                        upd.Parameters.AddWithValue("$sid", v.SourceId);
                        upd.Parameters.AddWithValue("$p", JsonSerializer.SerializeToUtf8Bytes(v.Payload));
                        upd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                        upd.Parameters.AddWithValue("$iid", intId);
                        await upd.ExecuteNonQueryAsync(cancellationToken);
                    }

                    // vec0 업데이트
                    using (var uvec = conn.CreateCommand())
                    {
                        uvec.CommandText = $@"UPDATE {vecTable} SET embedding = $emb WHERE rowid = $iid";
                        uvec.Parameters.AddWithValue("$emb", JsonSerializer.Serialize(v.Vectors));
                        uvec.Parameters.AddWithValue("$iid", intId);
                        await uvec.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
                else
                {
                    using (var ins = conn.CreateCommand())
                    {
                        ins.CommandText = $@"
                            INSERT INTO {metaTable}(vector_id, source_id, payload, last_upserted_at)
                            VALUES($vid, $sid, $p, $ts)";
                        ins.Parameters.AddWithValue("$vid", v.VectorId);
                        ins.Parameters.AddWithValue("$sid", v.SourceId);
                        ins.Parameters.AddWithValue("$p", JsonSerializer.SerializeToUtf8Bytes(v.Payload));
                        ins.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                        await ins.ExecuteNonQueryAsync(cancellationToken);
                    }

                    // 새 int_id
                    using (var last = conn.CreateCommand())
                    {
                        last.CommandText = "SELECT last_insert_rowid()";
                        intId = (long)(await last.ExecuteScalarAsync(cancellationToken) ?? 0L);
                    }

                    // vec0 삽입 (rowid를 int_id로 지정)
                    using (var ivec = conn.CreateCommand())
                    {
                        ivec.CommandText = $@"INSERT INTO {vecTable}(rowid, embedding) VALUES ($iid, $emb)";
                        ivec.Parameters.AddWithValue("$iid", intId);
                        ivec.Parameters.AddWithValue("$emb", JsonSerializer.Serialize(v.Vectors));
                        await ivec.ExecuteNonQueryAsync(cancellationToken);
                    }
                }
            }
        }

        await tx.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default)
    {
        using var conn = await CreateConnectionAsync();
        var vecTable = VecTable(collectionName);
        var metaTable = VecMetaTable(collectionName);

        if (filter == null || !filter.Any()) return;

        // 대상 int_id 집합
        var where = new List<string>();
        var args = new List<SqliteParameter>();

        if (filter.VectorIds.Count > 0)
        {
            where.Add($"vector_id IN ({JoinArrayParams("$vid", filter.VectorIds.Count)})");
            args.AddRange(filter.VectorIds.Select((v, i) => new SqliteParameter($"$vid{i}", v)));
        }
        if (filter.SourceIds.Count > 0)
        {
            where.Add($"source_id IN ({JoinArrayParams("$sid", filter.SourceIds.Count)})");
            args.AddRange(filter.SourceIds.Select((v, i) => new SqliteParameter($"$sid{i}", v)));
        }

        using var tx = await conn.BeginTransactionAsync(cancellationToken);

        var intIds = new List<long>();
        using (var idsCmd = conn.CreateCommand())
        {
            idsCmd.CommandText = $@"SELECT int_id FROM {metaTable} WHERE {string.Join(" AND ", where)}";
            idsCmd.Parameters.AddRange(args);

            using (var rdr = await idsCmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await rdr.ReadAsync(cancellationToken))
                    intIds.Add(rdr.GetInt64(0));
            }
        }

        if (intIds.Count == 0) { await tx.CommitAsync(cancellationToken); return; }

        using (var delVec = conn.CreateCommand())
        {
            delVec.CommandText = $@"DELETE FROM {vecTable} WHERE rowid IN ({JoinArrayParams("$iid", intIds.Count)})";
            foreach (var (val, i) in intIds.Select((v, i) => (v, i)))
                delVec.Parameters.AddWithValue($"$iid{i}", val);
            await delVec.ExecuteNonQueryAsync(cancellationToken);
        }

        using (var delMeta = conn.CreateCommand())
        {
            delMeta.CommandText = $@"DELETE FROM {metaTable} WHERE int_id IN ({JoinArrayParams("$iid", intIds.Count)})";
            foreach (var (val, i) in intIds.Select((v, i) => (v, i)))
                delMeta.Parameters.AddWithValue($"$iid{i}", val);
            await delMeta.ExecuteNonQueryAsync(cancellationToken);
        }

        await tx.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        float[] vector,
        float minScore = 0.0f,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        using var conn = await CreateConnectionAsync();

        var vecTable = VecTable(collectionName);
        var metaTable = VecMetaTable(collectionName);

        var where = new List<string>();
        var args = new List<SqliteParameter>();

        if (filter != null && filter.VectorIds.Count > 0)
        {
            where.Add($"m.vector_id IN ({JoinArrayParams("$vid", filter.VectorIds.Count)})");
            args.AddRange(filter.VectorIds.Select((v, i) => new SqliteParameter($"$vid{i}", v)));
        }
        if (filter != null && filter.SourceIds.Count > 0)
        {
            where.Add($"m.source_id IN ({JoinArrayParams("$sid", filter.SourceIds.Count)})");
            args.AddRange(filter.SourceIds.Select((v, i) => new SqliteParameter($"$sid{i}", v)));
        }

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT
                m.vector_id,
                m.source_id,
                m.payload,
                m.last_upserted_at,
                v.distance
            FROM {vecTable} v
            JOIN {metaTable} m ON m.int_id = v.rowid
            WHERE v.embedding MATCH $q
                AND k = $lim
                {(where.Count > 0 ? "AND " + string.Join(" AND ", where) : "")}
            ORDER BY v.distance";
        cmd.Parameters.AddWithValue("$q", JsonSerializer.Serialize(vector));
        cmd.Parameters.AddRange(args);
        cmd.Parameters.AddWithValue("$lim", limit > 4096 ? 4096 : limit); // vec0 4096 제한

        var records = new List<ScoredVectorRecord>();
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await rdr.ReadAsync(cancellationToken))
        {
            var distance = Convert.ToSingle(rdr.GetDouble(4));
            var score = 1.0f / (1.0f + distance); // 0~1로 변환
            if (score < minScore) continue;

            records.Add(new ScoredVectorRecord
            {
                Score = score,
                VectorId = rdr.GetString(0),
                SourceId = rdr.GetString(1),
                Payload = JsonSerializer.Deserialize<IDictionary<string, object?>>(
                    rdr.IsDBNull(2) ? null : (byte[])rdr["payload"])
                    ?? new Dictionary<string, object?>(),
                LastUpsertedAt = DateTimeOffset.Parse(rdr.GetString(3)).UtcDateTime,
                Vectors = [] // 메타 조회이므로 임베딩은 비워둠
            });
        }
        return records;
    }

    #region 내부 유틸

    private static string VecTable(string name) => $"vec_{name}";
    private static string VecMetaTable(string name) => $"vec_{name}_meta";

    private static string JoinArrayParams(string prefix, int count)
        => string.Join(", ", Enumerable.Range(0, count).Select(i => $"{prefix}{i}"));

    /// <summary> sqlite-vec 모듈이 로드된 연결을 생성합니다. </summary>
    private async Task<SqliteConnection> CreateConnectionAsync()
    {
        // 데이터베이스 연결
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();

        // PRAGMA 튜닝 (WAL 지속 유지)
        using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = @"
            PRAGMA journal_mode=WAL;
            PRAGMA synchronous=NORMAL;
            PRAGMA temp_store=MEMORY;";
            await pragma.ExecuteNonQueryAsync();
        }

        // vec0 모듈 로드 확인
        if (await HasVec0Async(conn))
            return conn;

        // vec0 모듈 설치
        var dirPath = Path.GetDirectoryName(conn.DataSource)
            ?? throw new InvalidOperationException("데이터베이스 경로를 찾을 수 없습니다.");
        var modulePath = SqliteVecInstaller.TryGetModule(dirPath, out var module) && module.Version == _moduleVersion
            ? module.FilePath
            : await SqliteVecInstaller.InstallAsync(dirPath, _moduleVersion);

        // vec0 모듈 로드
        conn.EnableExtensions(true);
        conn.LoadExtension(modulePath);

        return conn;
    }

    /// <summary> vec0 모듈이 로드되었는지 확인합니다. </summary>
    private static async Task<bool> HasVec0Async(SqliteConnection conn)
    {
        try
        {
            // vec0에 있는 가장 가벼운 함수 호출 시도
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT vec_version()"; 
            await cmd.ExecuteScalarAsync();
            return true;
        }
        catch
        {
            // 함수가 없거나 미로딩 상태
            return false;
        }
    }

    /// <summary> 영문자, 숫자, 밑줄만 허용하며 숫자로 시작할 수 없는 컬렉션 이름 정규식 </summary>
    [GeneratedRegex(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled)]
    private static partial Regex CollectionNameRegex();

    /// <summary> 컬렉션 이름 유효성 검사 및 정규화 </summary>
    private static string EnsureCollectionName(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentNullException("collection name can not be null");

        if (!CollectionNameRegex().IsMatch(collectionName))
            throw new ArgumentException("컬렉션 이름은 영문자/숫자/밑줄만 가능하며 숫자로 시작할 수 없습니다.", nameof(collectionName));

        if (collectionName.Equals(CollectionMetaTable, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("컬렉션 이름이 예약어와 충돌합니다.", nameof(collectionName));

        // Sqlite는 대소문자를 구분하지 않습니다.
        return collectionName.ToLowerInvariant();
    }

    /// <summary> CollectionMetaTable 테이블이 없으면 생성합니다. (Thread-safe with double-checked locking) </summary>
    private async Task EnsureCollectionMetaTableAsync()
    {
        if (_ensuredCollMetaTable) return;

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_ensuredCollMetaTable) return; // Double-check after acquiring lock

            using var conn = await CreateConnectionAsync().ConfigureAwait(false);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                CREATE TABLE IF NOT EXISTS {CollectionMetaTable}(
                    name TEXT PRIMARY KEY,
                    dimensions INTEGER NOT NULL,
                    embedding_provider TEXT NOT NULL,
                    embedding_model TEXT NOT NULL
                )";
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            _ensuredCollMetaTable = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    #endregion
}
