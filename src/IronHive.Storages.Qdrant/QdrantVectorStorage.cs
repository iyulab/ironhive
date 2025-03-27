using System.Text.Json;
using IronHive.Abstractions.Memory;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace IronHive.Storages.Qdrant;

public class QdrantVectorStorage : IVectorStorage
{
    // Qdrant에서 사용할 기본 텍스트용 벡터의 이름입니다.
    private const string DefaultVectorsName = "text_vector";
    private readonly QdrantClient _client;

    public QdrantVectorStorage(QdrantConfig config)
    {
        _client = CreateQdrantClient(config);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> ListCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _client.ListCollectionsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        return await _client.CollectionExistsAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        string collectionName,
        int dimensions,
        CancellationToken cancellationToken = default)
    {
        if (await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} already exist");

        var param = new VectorParams
        {
            Datatype = Datatype.Float32,
            Distance = Distance.Cosine,
            OnDisk = true,
            Size = (ulong)dimensions
        };
        var config = new VectorParamsMap { Map = { [DefaultVectorsName] = param } };
        await _client.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: config,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} is not exist");

        await _client.DeleteCollectionAsync(
            collectionName: collectionName,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName,
        int limit = 20,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var condition = BuildFilter(filter);
        var orderBy = new OrderBy { Key = "lastUpdatedAt", Direction = Direction.Desc };
        var response = await _client.ScrollAsync(
            collectionName: collectionName,
            limit: (uint)limit,
            filter: condition,
            orderBy: orderBy,
            cancellationToken: cancellationToken);

        var records = new List<VectorRecord>();
        foreach (var point in response.Result)
        {
            var vectors = point.Vectors.Vectors_.Vectors[DefaultVectorsName].Data.ToArray();

            var vectorId = point.Payload.GetValueOrDefault("vectorId")?.StringValue ?? string.Empty;
            var source = JsonSerializer.Deserialize<IMemorySource>(point.Payload.GetValueOrDefault("source")?.StringValue ?? string.Empty)
                ?? throw new InvalidOperationException("source is not found");
            var payload = JsonSerializer.Deserialize<object>(point.Payload.GetValueOrDefault("payload")?.StringValue ?? string.Empty);
            var lastUpdatedAt = DateTime.Parse(point.Payload.GetValueOrDefault("lastUpdatedAt")?.StringValue ?? string.Empty);
            
            records.Add(new VectorRecord
            {
                Id = vectorId,
                Vectors = vectors,
                Source = source,
                Payload = payload,
                LastUpdatedAt = lastUpdatedAt
            });
        }
        return records;
    }

    /// <inheritdoc />
    public async Task UpsertVectorsAsync(
        string collectionName,
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default)
    {
        var points = new List<PointStruct>();
        foreach (var record in records)
        {
            points.Add(new PointStruct
            {
                Vectors = new Dictionary<string, float[]>
                {
                    [DefaultVectorsName] = record.Vectors.ToArray(),
                },
                Payload =
                {
                    ["vectorId"] = record.Id,
                    ["sourceId"] = record.Source.Id,
                    ["source"] = JsonSerializer.Serialize(record.Source),
                    ["payload"] = JsonSerializer.Serialize(record.Payload),
                    ["lastUpdatedAt"] = record.LastUpdatedAt.ToString("O"),
                }
            });
        }

        await _client.UpsertAsync(
            collectionName: collectionName,
            points: points,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteVectorsAsync(
        string collectionName,
        VectorRecordFilter filter,
        CancellationToken cancellationToken = default)
    {
        var condition = BuildFilter(filter)
            ?? throw new InvalidOperationException("filter is required");

        await _client.DeleteAsync(
            collectionName: collectionName,
            filter: condition,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        IEnumerable<float> vector,
        float minScore = 0.0f,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        var condition = BuildFilter(filter);

        var scoredPoints = await _client.SearchAsync(
            collectionName: collectionName,
            vector: vector.ToArray(),
            filter: condition,
            limit: (ulong)limit,
            payloadSelector: true,
            vectorsSelector: false,
            scoreThreshold: minScore,
            vectorName: DefaultVectorsName,
            cancellationToken: cancellationToken);

        var records = new List<ScoredVectorRecord>();
        foreach (var point in scoredPoints)
        {
            var vectorId = point.Payload.GetValueOrDefault("vectorId")?.StringValue ?? string.Empty;
            var source = JsonSerializer.Deserialize<IMemorySource>(point.Payload.GetValueOrDefault("source")?.StringValue ?? string.Empty)
                ?? throw new InvalidOperationException("source is not found");
            var payload = JsonSerializer.Deserialize<object>(point.Payload.GetValueOrDefault("payload")?.StringValue ?? string.Empty);
            var lastUpdatedAt = DateTime.Parse(point.Payload.GetValueOrDefault("lastUpdatedAt")?.StringValue ?? string.Empty);

            records.Add(new ScoredVectorRecord
            {
                VectorId = vectorId,
                Score = point.Score,
                Source = source,
                LastUpdatedAt = lastUpdatedAt,
            });
        }

        return records;
    }

    // 필터링 조건을 생성합니다.
    private Filter? BuildFilter(VectorRecordFilter? filter)
    {
        if (filter == null || filter.Any())
            return null;

        var conditions = new List<Condition>();
        if (filter.VectorIds.Count > 0)
        {
            conditions.Add(Match("vectorId", filter.VectorIds.ToArray()));
        }
        if (filter.SourceIds.Count > 0)
        {
            conditions.Add(Match("sourceId", filter.SourceIds.ToArray()));
        }

        // 모든 조건을 OR 연산으로 결합합니다.
        return conditions.Aggregate((current, next) => current | next);
    }

    // QdrantClient 인스턴스를 생성합니다.
    private QdrantClient CreateQdrantClient(QdrantConfig config)
    {
        return new QdrantClient(
            host: config.Host,
            port: config.Port,
            https: config.Https,
            apiKey: config.ApiKey,
            grpcTimeout: config.GrpcTimeout);
    }
}
