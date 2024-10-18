using LiteDB;
using Raggle.Abstractions.Memory;

namespace Raggle.Vector.LiteDB;

public class LiteDBVectorStorage : IVectorStorage, IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteCollection<VectorRecord> _collection;

    public LiteDBVectorStorage(string databasePath = "vectors.db")
    {
        _database = new LiteDatabase(databasePath);
        _collection = _database.GetCollection<VectorRecord>("vector_records");
        _collection.EnsureIndex(x => x.DocumentId, true);
    }

    public Task CreateCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        // LiteDB는 스키마리스이므로 컬렉션을 미리 생성할 필요 없음
        // 단, 컬렉션 이름을 변경하거나 여러 컬렉션을 관리하려면 추가 로직 필요
        return Task.CompletedTask;
    }

    public Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        _database.DropCollection("vector_records");
        return Task.CompletedTask;
    }

    public Task<IEnumerable<VectorRecord>> FindRecordsAsync(string collectionName, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        // 필터링 로직을 구현해야 함. 여기서는 단순히 모든 레코드를 반환
        var records = _collection.FindAll();
        return Task.FromResult<IEnumerable<VectorRecord>>(records);
    }

    public Task UpsertRecordAsync(string collectionName, VectorRecord record, CancellationToken cancellationToken = default)
    {
        _collection.Upsert(record);
        return Task.CompletedTask;
    }

    public Task UpsertRecordsAsync(string collectionName, IEnumerable<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        _collection.Upsert(records);
        return Task.CompletedTask;
    }

    public Task DeleteRecordsAsync(string collectionName, string documentId, CancellationToken cancellationToken = default)
    {
        _collection.Delete(documentId);
        return Task.CompletedTask;
    }

    public Task SearchRecordsAsync(string collectionName, float[] input, int limit = 5, CancellationToken cancellationToken = default)
    {
        // 간단한 코사인 유사도 기반 검색 구현
        var allRecords = _collection.FindAll();
        var results = allRecords
            .Select(record => new
            {
                Record = record,
                Score = CosineSimilarity(input, record.Embedding)
            })
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Record);
        return Task.CompletedTask;
    }

    private float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be the same length");

        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0;

        return dotProduct / ((float)Math.Sqrt(magnitudeA) * (float)Math.Sqrt(magnitudeB));
    }

    public void Dispose()
    {
        _database?.Dispose();
    }
}
