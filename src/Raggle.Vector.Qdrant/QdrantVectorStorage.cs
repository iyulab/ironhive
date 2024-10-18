using Qdrant.Client;
using Qdrant.Client.Grpc;
using Raggle.Abstractions.Memory;

namespace Raggle.Vector.Qdrant;

public class QdrantVectorStorage : IVectorStorage
{
    private readonly QdrantClient _client;

    public QdrantVectorStorage(string connectionString = "http://localhost:6333")
    {
        _client = new QdrantClient(connectionString);
    }

    public async Task CreateCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        var config = new VectorParams
        {
            Datatype = Datatype.Float16, // 데이터 타입
            Distance = Distance.Cosine,
            Size = 128, // 임베딩 차원수
        };
        await _client.CreateCollectionAsync(
            collectionName: collection, 
            vectorsConfig: config,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteCollectionAsync(string collection, CancellationToken cancellationToken = default)
    {
        await _client.DeleteCollectionAsync(collectionName: collection, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<VectorRecord>> FindRecordsAsync(string collection, MemoryFilter? filter = null, CancellationToken cancellationToken = default)
    {
        //var response = await _client.SearchAsync(
        //    collectionName: collection,
            
        //    cancellationToken: cancellationToken);

        //var records = new List<VectorRecord>();
        //foreach (var hit in response.Result.Hits)
        //{
        //    records.Add(new VectorRecord
        //    {
        //        DocumentId = hit.Id,
        //        Embedding = hit.Vector.ToArray(),
        //        // 필요한 필드 매핑
        //    });
        //}

        //return records;
        throw new NotImplementedException();
    }

    public async Task UpsertRecordAsync(string collection, VectorRecord record, CancellationToken cancellationToken = default)
    {
        var point = new PointStruct
        {
            Vectors = record.Embedding,
            Payload =
            {
                ["documentId"] = record.DocumentId,
                ["tags"] = record.Tags ?? [],
                ["content"] = record.Content ?? string.Empty
            }
        };

        await _client.UpsertAsync(
            collectionName: collection,
            points: [ point ],
            cancellationToken: cancellationToken);
    }

    public async Task UpsertRecordsAsync(string collection, IEnumerable<VectorRecord> records, CancellationToken cancellationToken = default)
    {
        var points = new List<PointStruct>();
        foreach (var record in records)
        {
            points.Add(new PointStruct
            {
                Vectors = record.Embedding,
                Payload =
                {
                    ["documentId"] = record.DocumentId,
                    ["tags"] = record.Tags ?? [],
                    ["content"] = record.Content ?? string.Empty
                }
            });
        }

        await _client.UpsertAsync(
            collectionName: collection,
            points: points,
            cancellationToken: cancellationToken);
    }

    public async Task DeleteRecordsAsync(string collection, string documentId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task SearchRecordsAsync(string collection, float[] input, int limit = 5, CancellationToken cancellationToken = default)
    {
        var response = await _client.SearchAsync(
            collectionName: collection,
            vector: input,
            limit: (ulong)limit,
            cancellationToken: cancellationToken);
    }
}
