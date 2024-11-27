using Microsoft.Data.Sqlite;
using Raggle.Abstractions.Memory;
using System.Numerics.Tensors;
using System.Text.Json;

namespace Raggle.Driver.Sqlite;

public class SqliteVectorStorage : IVectorStorage
{
    private readonly SqliteConnection _connection;

    public SqliteVectorStorage(SqliteConfig config)
    {
        _connection = CreateSqliteConnection(config);
        InitializeDatabaseAsync().GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<IEnumerable<string>> GetCollectionListAsync(
        CancellationToken cancellationToken = default)
    {
        var collections = new List<string>();
        var query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

        using var command = new SqliteCommand(query, _connection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            collections.Add(reader.GetString(0));
        }

        return collections;
    }

    public async Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var query = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name;";
        using var command = new SqliteCommand(query, _connection);
        command.Parameters.AddWithValue("@name", collectionName);
        var count = (long)await command.ExecuteScalarAsync(cancellationToken);
        return count > 0;
    }

    public async Task CreateCollectionAsync(
        string collectionName,
        int vectorSize,
        CancellationToken cancellationToken = default)
    {
        if (await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' already exists.");

        var createTableQuery = $@"
                CREATE TABLE [{collectionName}] (
                    VectorId TEXT PRIMARY KEY,
                    DocumentId TEXT NOT NULL,
                    Vectors BLOB NOT NULL,
                    Tags TEXT,
                    ChunkIndex INTEGER,
                    QAPairIndex INTEGER
                );
                CREATE INDEX IDX_{collectionName}_DocumentId ON [{collectionName}](DocumentId);
                CREATE INDEX IDX_{collectionName}_Tags ON [{collectionName}](Tags);
            ";

        using var command = new SqliteCommand(createTableQuery, _connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        var dropTableQuery = $"DROP TABLE [{collectionName}];";
        using var command = new SqliteCommand(dropTableQuery, _connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteVectorsAsync(
        string collectionName,
        string documentId,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        var deleteQuery = $"DELETE FROM [{collectionName}] WHERE DocumentId = @documentId;";
        using var command = new SqliteCommand(deleteQuery, _connection);
        command.Parameters.AddWithValue("@documentId", documentId);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorPoint> points,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        using var transaction = _connection.BeginTransaction();
        var upsertQuery = $@"
                INSERT INTO [{collectionName}] (VectorId, DocumentId, Vectors, Tags, ChunkIndex, QAPairIndex)
                VALUES (@VectorId, @DocumentId, @Vectors, @Tags, @ChunkIndex, @QAPairIndex)
                ON CONFLICT(VectorId) DO UPDATE SET
                    DocumentId = excluded.DocumentId,
                    Vectors = excluded.Vectors,
                    Tags = excluded.Tags,
                    ChunkIndex = excluded.ChunkIndex,
                    QAPairIndex = excluded.QAPairIndex;
            ";

        foreach (var point in points)
        {
            using var command = new SqliteCommand(upsertQuery, _connection, transaction);
            command.Parameters.AddWithValue("@VectorId", point.VectorId);
            command.Parameters.AddWithValue("@DocumentId", point.DocumentId);
            command.Parameters.AddWithValue("@Vectors", SerializeVector(point.Vectors));
            command.Parameters.AddWithValue("@Tags", point.Tags != null ? JsonSerializer.Serialize(point.Tags) : null);
            command.Parameters.AddWithValue("@ChunkIndex", point.ChunkIndex);
            command.Parameters.AddWithValue("@QAPairIndex", point.QAPairIndex);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IEnumerable<ScoredVectorPoint>> SearchVectorsAsync(
        string collectionName,
        float[] input,
        float minScore = 0,
        int limit = 5,
        MemoryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"Collection '{collectionName}' not found.");

        var vectors = new List<VectorPoint>();

        var selectQuery = $"SELECT VectorId, DocumentId, Vectors, Tags, ChunkIndex, QAPairIndex FROM [{collectionName}]";

        var conditions = new List<string>();
        var parameters = new List<SqliteParameter>();

        if (filter != null)
        {
            if (filter.DocumentIds.Count > 0)
            {
                var docParams = string.Join(", ", filter.DocumentIds.Select((_, i) => $"@docId{i}"));
                conditions.Add($"DocumentId IN ({docParams})");
                for (int i = 0; i < filter.DocumentIds.Count; i++)
                {
                    parameters.Add(new SqliteParameter($"@docId{i}", filter.DocumentIds[i]));
                }
            }

            if (filter.Tags.Count > 0)
            {
                var tagConditions = string.Join(" OR ", filter.Tags.Select((_, i) => $"Tags LIKE @tag{i}"));
                conditions.Add($"({tagConditions})");
                for (int i = 0; i < filter.Tags.Count; i++)
                {
                    parameters.Add(new SqliteParameter($"@tag{i}", $"%\"{filter.Tags[i]}\"%"));
                }
            }
        }

        if (conditions.Any())
        {
            selectQuery += " WHERE " + string.Join(" AND ", conditions);
        }

        using var command = new SqliteCommand(selectQuery, _connection);
        command.Parameters.AddRange(parameters.ToArray());

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var vectorPoint = new VectorPoint
            {
                VectorId = Guid.Parse(reader.GetString(0)),
                DocumentId = reader.GetString(1),
                Vectors = DeserializeVector(reader.GetString(2)),
                Tags = reader.IsDBNull(3) ? [] : JsonSerializer.Deserialize<string[]>(reader.GetString(3)),
                ChunkIndex = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                QAPairIndex = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
            };
            vectors.Add(vectorPoint);
        }

        // Compute cosine similarity
        var results = vectors
            .Select(p => new ScoredVectorPoint
            {
                VectorId = p.VectorId,
                Score = TensorPrimitives.CosineSimilarity(input, p.Vectors),
                DocumentId = p.DocumentId,
                ChunkIndex = p.ChunkIndex,
                QAPairIndex = p.QAPairIndex,
            })
            .Where(p => p.Score >= minScore)
            .OrderByDescending(p => p.Score)
            .Take(limit);

        return results;
    }

    #region Private Methods

    private static SqliteConnection CreateSqliteConnection(SqliteConfig config)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = config.DatabasePath,
            Password = config.Password,
            Mode = SqliteOpenMode.ReadWriteCreate,
        };

        var connection = new SqliteConnection(connectionStringBuilder.ToString());
        connection.Open();
        return connection;
    }

    private async Task InitializeDatabaseAsync()
    {
        await Task.CompletedTask;
    }

    private string SerializeVector(float[] vectors)
    {
        return JsonSerializer.Serialize(vectors);
    }

    private float[] DeserializeVector(string vectorString)
    {
        return JsonSerializer.Deserialize<float[]>(vectorString) ?? Array.Empty<float>();
    }

    #endregion
}
