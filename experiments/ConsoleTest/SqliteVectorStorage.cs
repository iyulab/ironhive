using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IronHive.Abstractions.Memory;
using IronHive.Abstractions.Vector;
using Microsoft.Data.Sqlite;

namespace ConsoleTest;

/// <summary>
/// sqlite-vec(vec0) 기반 IVectorStorage 구현체
/// </summary>
public sealed class SqliteVecVectorStorage : IVectorStorage
{
    private readonly string _connectionString;
    private readonly Regex _safeName = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    public SqliteVecVectorStorage(string databasePath)
    {
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ConnectionString;

        // 스키마 보장
        EnsureBaseSchemaAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose
    }

    public async Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(CancellationToken cancellationToken = default)
    {
        using var conn = await CreateConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $@"SELECT name, dimensions, embedding_provider, embedding_model
                            FROM {CollectionMetaTable}
                            ORDER BY name";
        var list = new List<VectorCollectionInfo>();
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
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

    public async Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        using var conn = await CreateConnectionAsync();
        return await GetCollectionInfoInternalAsync(conn, collectionName, cancellationToken) != null;
    }

    public async Task<VectorCollectionInfo?> GetCollectionInfoAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        using var conn = await CreateConnectionAsync();
        return await GetCollectionInfoInternalAsync(conn, collectionName, cancellationToken);
    }

    public async Task CreateCollectionAsync(VectorCollectionInfo collection, CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collection.Name);
        if (collection.Dimensions <= 0) throw new ArgumentOutOfRangeException(nameof(collection.Dimensions));

        using var conn = await CreateConnectionAsync();
        using var tx = conn.BeginTransaction();

        // 메타 등록
        {
            var insert = conn.CreateCommand();
            insert.CommandText = $@"
                INSERT INTO {CollectionMetaTable}(name, dimensions, embedding_provider, embedding_model)
                VALUES ($n, $d, $p, $m)";
            insert.Parameters.AddWithValue("$n", collection.Name);
            insert.Parameters.AddWithValue("$d", collection.Dimensions);
            insert.Parameters.AddWithValue("$p", collection.EmbeddingProvider);
            insert.Parameters.AddWithValue("$m", collection.EmbeddingModel);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        // 컬렉션별 일반 테이블
        var metaTable = VecMetaTable(collection.Name);
        {
            var sql = $@"
                CREATE TABLE IF NOT EXISTS {metaTable}(
                    int_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    id TEXT NOT NULL UNIQUE,
                    source_id TEXT NOT NULL,
                    content TEXT NULL,
                    last_upserted_at TEXT NOT NULL
                )";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            // 인덱스
            cmd = conn.CreateCommand();
            cmd.CommandText = $@"CREATE INDEX IF NOT EXISTS idx_{metaTable}_source ON {metaTable}(source_id)";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
            cmd = conn.CreateCommand();
            cmd.CommandText = $@"CREATE INDEX IF NOT EXISTS idx_{metaTable}_last ON {metaTable}(last_upserted_at DESC)";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        // 컬렉션별 vec0 테이블 (차원 지정)
        var vecTable = VecTable(collection.Name);
        {
            var sql = $@"CREATE VIRTUAL TABLE IF NOT EXISTS {vecTable}
                         USING vec0(embedding float[{collection.Dimensions}])";
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        tx.Commit();
    }

    public async Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        using var conn = await CreateConnectionAsync();
        using var tx = conn.BeginTransaction();

        var metaTable = VecMetaTable(collectionName);
        var vecTable = VecTable(collectionName);

        async Task DropIfExists(string type, string name)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"DROP {type} IF EXISTS {name}";
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        await DropIfExists("TABLE", metaTable);
        await DropIfExists("TABLE", $"sqlite_sequence"); // 오토인크리먼트 시퀀스는 공용이라 주의
        await DropIfExists("TABLE", $"idx_{metaTable}_source"); // 인덱스 드랍은 자동으로 처리되나 안전차원에서
        await DropIfExists("TABLE", $"idx_{metaTable}_last");
        await DropIfExists("VIRTUAL TABLE", vecTable);

        var delMeta = conn.CreateCommand();
        delMeta.CommandText = $"DELETE FROM {CollectionMetaTable} WHERE name=$n";
        delMeta.Parameters.AddWithValue("$n", collectionName);
        await delMeta.ExecuteNonQueryAsync(cancellationToken);

        tx.Commit();
    }

    public async Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        using var conn = await CreateConnectionAsync();
        var metaTable = VecMetaTable(collectionName);

        var where = new List<string>();
        var args = new List<SqliteParameter>();

        if (filter != null && filter.VectorIds.Count > 0)
        {
            where.Add($"id IN ({JoinArrayParams("$vid", filter.VectorIds.Count)})");
            args.AddRange(filter.VectorIds.Select((v, i) => new SqliteParameter($"$vid{i}", v)));
        }
        if (filter != null && filter.SourceIds.Count > 0)
        {
            where.Add($"source_id IN ({JoinArrayParams("$sid", filter.SourceIds.Count)})");
            args.AddRange(filter.SourceIds.Select((v, i) => new SqliteParameter($"$sid{i}", v)));
        }

        var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            SELECT id, source_id, content, last_upserted_at
            FROM {metaTable}
            {(where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "")}
            ORDER BY datetime(last_upserted_at) DESC
            LIMIT $lim";
        foreach (var p in args) cmd.Parameters.Add(p);
        cmd.Parameters.AddWithValue("$lim", limit);

        var list = new List<VectorRecord>();
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await rdr.ReadAsync(cancellationToken))
        {
            list.Add(new VectorRecord
            {
                Id = rdr.GetString(0),
                Source = new TextMemorySource { Value = ""},
                Content = TryDeserialize(rdr.IsDBNull(2) ? null : rdr.GetString(2)),
                LastUpsertedAt = DateTime.Parse(rdr.GetString(3)).ToUniversalTime(),
                Vectors = Array.Empty<float>() // 메타 조회이므로 임베딩은 비워둠
            });
        }
        return list;
    }

    public async Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> vectors,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        using var conn = await CreateConnectionAsync();
        var vecTable = VecTable(collectionName);
        var metaTable = VecMetaTable(collectionName);

        using var tx = conn.BeginTransaction();

        foreach (var v in vectors)
        {
            if (v.Vectors is null) throw new ArgumentException("Vectors가 null입니다.", nameof(vectors));
            if (string.IsNullOrWhiteSpace(v.Id)) throw new ArgumentException("Id가 필요합니다.", nameof(vectors));
            if (v.Source is null) throw new ArgumentException("Source가 필요합니다.", nameof(vectors));

            // meta upsert
            long intId;
            {
                // 먼저 존재여부 확인
                var sel = conn.CreateCommand();
                sel.CommandText = $@"SELECT int_id FROM {metaTable} WHERE id=$id";
                sel.Parameters.AddWithValue("$id", v.Id);
                var existing = await sel.ExecuteScalarAsync(cancellationToken);

                if (existing is long l)
                {
                    intId = l;

                    var upd = conn.CreateCommand();
                    upd.CommandText = $@"
                        UPDATE {metaTable}
                        SET source_id=$sid, content=$c, last_upserted_at=$ts
                        WHERE int_id=$iid";
                    upd.Parameters.AddWithValue("$sid", v.Source.Id);
                    upd.Parameters.AddWithValue("$c", v.Content is null ? (object?)DBNull.Value : JsonSerializer.Serialize(v.Content));
                    upd.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                    upd.Parameters.AddWithValue("$iid", intId);
                    await upd.ExecuteNonQueryAsync(cancellationToken);

                    // vec0 업데이트
                    var uvec = conn.CreateCommand();
                    uvec.CommandText = $@"UPDATE {vecTable} SET embedding = $emb WHERE rowid = $iid";
                    uvec.Parameters.AddWithValue("$emb", JsonSerializer.Serialize(v.Vectors));
                    uvec.Parameters.AddWithValue("$iid", intId);
                    await uvec.ExecuteNonQueryAsync(cancellationToken);
                }
                else
                {
                    var ins = conn.CreateCommand();
                    ins.CommandText = $@"
                        INSERT INTO {metaTable}(id, source_id, content, last_upserted_at)
                        VALUES($id, $sid, $c, $ts)";
                    ins.Parameters.AddWithValue("$id", v.Id);
                    ins.Parameters.AddWithValue("$sid", v.Source.Id);
                    ins.Parameters.AddWithValue("$c", v.Content is null ? (object?)DBNull.Value : JsonSerializer.Serialize(v.Content));
                    ins.Parameters.AddWithValue("$ts", DateTime.UtcNow.ToString("o"));
                    await ins.ExecuteNonQueryAsync(cancellationToken);

                    // 새 int_id
                    var last = conn.CreateCommand();
                    last.CommandText = "SELECT last_insert_rowid()";
                    intId = (long)(await last.ExecuteScalarAsync(cancellationToken) ?? 0L);

                    // vec0 삽입 (rowid를 int_id로 지정)
                    var ivec = conn.CreateCommand();
                    ivec.CommandText = $@"INSERT INTO {vecTable}(rowid, embedding) VALUES ($iid, $emb)";
                    ivec.Parameters.AddWithValue("$iid", intId);
                    ivec.Parameters.AddWithValue("$emb", JsonSerializer.Serialize(v.Vectors));
                    await ivec.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        tx.Commit();
    }

    public async Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        using var conn = await CreateConnectionAsync();
        var vecTable = VecTable(collectionName);
        var metaTable = VecMetaTable(collectionName);

        if (filter == null || !filter.Any()) return;

        // 대상 int_id 집합
        var where = new List<string>();
        var args = new List<SqliteParameter>();

        if (filter.VectorIds.Count > 0)
        {
            where.Add($"id IN ({JoinArrayParams("$vid", filter.VectorIds.Count)})");
            args.AddRange(filter.VectorIds.Select((v, i) => new SqliteParameter($"$vid{i}", v)));
        }
        if (filter.SourceIds.Count > 0)
        {
            where.Add($"source_id IN ({JoinArrayParams("$sid", filter.SourceIds.Count)})");
            args.AddRange(filter.SourceIds.Select((v, i) => new SqliteParameter($"$sid{i}", v)));
        }

        using var tx = conn.BeginTransaction();

        var idsCmd = conn.CreateCommand();
        idsCmd.CommandText = $@"SELECT int_id FROM {metaTable} WHERE {string.Join(" AND ", where)}";
        foreach (var p in args) idsCmd.Parameters.Add(p);

        var intIds = new List<long>();
        using (var rdr = await idsCmd.ExecuteReaderAsync(cancellationToken))
            while (await rdr.ReadAsync(cancellationToken))
                intIds.Add(rdr.GetInt64(0));

        if (intIds.Count == 0) { tx.Commit(); return; }

        var delVec = conn.CreateCommand();
        delVec.CommandText = $@"DELETE FROM {vecTable} WHERE rowid IN ({JoinArrayParams("$iid", intIds.Count)})";
        foreach (var (val, i) in intIds.Select((v, i) => (v, i)))
            delVec.Parameters.AddWithValue($"$iid{i}", val);
        await delVec.ExecuteNonQueryAsync(cancellationToken);

        var delMeta = conn.CreateCommand();
        delMeta.CommandText = $@"DELETE FROM {metaTable} WHERE int_id IN ({JoinArrayParams("$iid", intIds.Count)})";
        foreach (var (val, i) in intIds.Select((v, i) => (v, i)))
            delMeta.Parameters.AddWithValue($"$iid{i}", val);
        await delMeta.ExecuteNonQueryAsync(cancellationToken);

        tx.Commit();
    }

    public async Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> vector,
        float minScore = 0.0f,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        ValidateCollectionName(collectionName);
        using var conn = await CreateConnectionAsync();

        var vecTable = VecTable(collectionName);
        var metaTable = VecMetaTable(collectionName);

        var where = new List<string>();
        var args = new List<SqliteParameter>();

        if (filter != null && filter.VectorIds.Count > 0)
        {
            where.Add($"m.id IN ({JoinArrayParams("$vid", filter.VectorIds.Count)})");
            args.AddRange(filter.VectorIds.Select((v, i) => new SqliteParameter($"$vid{i}", v)));
        }
        if (filter != null && filter.SourceIds.Count > 0)
        {
            where.Add($"m.source_id IN ({JoinArrayParams("$sid", filter.SourceIds.Count)})");
            args.AddRange(filter.SourceIds.Select((v, i) => new SqliteParameter($"$sid{i}", v)));
        }

        // vec0 KNN: WHERE embedding MATCH ? ORDER BY distance LIMIT ?
        var cmd = conn.CreateCommand();
        var jsonVec = JsonSerializer.Serialize(vector);
        cmd.CommandText = $@"
            SELECT 
                m.id,
                m.source_id,
                m.content,
                m.last_upserted_at,
                v.distance
            FROM {vecTable} v
            JOIN {metaTable} m ON m.int_id = v.rowid
            WHERE v.embedding MATCH $q
                AND k = $lim
                {(where.Count > 0 ? "AND " + string.Join(" AND ", where) : "")}
            ORDER BY v.distance
            LIMIT $lim";
        cmd.Parameters.AddWithValue("$q", jsonVec);
        foreach (var p in args) cmd.Parameters.Add(p);
        cmd.Parameters.AddWithValue("$lim", limit);

        var results = new List<ScoredVectorRecord>();
        using var rdr = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await rdr.ReadAsync(cancellationToken))
        {
            var distance = Convert.ToSingle(rdr.GetDouble(4));
            var score = 1.0f / (1.0f + distance); // 0~1로 변환
            if (score < minScore) continue;

            results.Add(new ScoredVectorRecord
            {
                VectorId = rdr.GetString(0),
                Source = new TextMemorySource { Value = ""},
                Content = TryDeserialize(rdr.IsDBNull(2) ? null : rdr.GetString(2)),
                LastUpdatedAt = DateTime.Parse(rdr.GetString(3)).ToUniversalTime(),
                Score = score
            });
        }
        return results;
    }

    #region 내부 유틸

    private async Task<SqliteConnection> CreateConnectionAsync()
    {
        // 데이터베이스 연결
        var conn = new SqliteConnection(_connectionString);
        conn.Open();

        // vec0 모듈 설치
        var dirPath = Path.GetDirectoryName(conn.DataSource)
            ?? throw new InvalidOperationException("데이터베이스 경로를 찾을 수 없습니다.");
        if (!SqliteVecInstaller.TryGetModule(dirPath, out var modulePath, out _))
        { 
            modulePath = await SqliteVecInstaller.InstallAsync(dirPath);
        }

        // vec0 확장 로드
        conn.EnableExtensions(true);
        conn.LoadExtension(modulePath);
        return conn;
    }

    private static object? TryDeserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<object>(json); }
        catch { return json; } // 안전장치
    }

    private static string JoinArrayParams(string prefix, int count)
        => string.Join(", ", Enumerable.Range(0, count).Select(i => $"{prefix}{i}"));

    private static string VecTable(string name) => $"vec_{name}";
    private static string VecMetaTable(string name) => $"vec_{name}_meta";
    private static string CollectionMetaTable => "vec_collections";

    private void ValidateCollectionName(string name)
    {
        if (!_safeName.IsMatch(name))
            throw new ArgumentException("컬렉션 이름은 영문자/숫자/밑줄만 가능하며 숫자로 시작할 수 없습니다.", nameof(name));
    }

    private static async Task<VectorCollectionInfo?> GetCollectionInfoInternalAsync(SqliteConnection conn, string collectionName, CancellationToken ct)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = $@"SELECT name, dimensions, embedding_provider, embedding_model
                            FROM {CollectionMetaTable} WHERE name=$n";
        cmd.Parameters.AddWithValue("$n", collectionName);
        using var rdr = await cmd.ExecuteReaderAsync(ct);
        if (await rdr.ReadAsync(ct))
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

    private async Task EnsureBaseSchemaAsync()
    {
        using var conn = await CreateConnectionAsync();
        var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
            CREATE TABLE IF NOT EXISTS {CollectionMetaTable}(
                name TEXT PRIMARY KEY,
                dimensions INTEGER NOT NULL,
                embedding_provider TEXT NOT NULL,
                embedding_model TEXT NOT NULL
            )";
        cmd.ExecuteNonQuery();
    }

    #endregion
}
