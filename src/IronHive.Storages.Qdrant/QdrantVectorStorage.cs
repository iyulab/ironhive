using System.Text.Json;
using Google.Protobuf.Collections;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Memory;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace IronHive.Storages.Qdrant;

public class QdrantVectorStorage : IVectorStorage
{
    // Qdrant에서 사용할 기본 텍스트용 벡터의 이름입니다.
    private const string DefaultVectorsName = "text";
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
        string? prefix = null,
        CancellationToken cancellationToken = default)
    {
        prefix ??= string.Empty;
        var colls = await _client.ListCollectionsAsync(cancellationToken);
        return colls.Where(c => c.StartsWith(prefix));
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
        collectionName = EnsureCollectionName(collectionName);

        if (await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} already exist");

        var param = new VectorParams
        {
            Datatype = Datatype.Float32,
            Distance = Distance.Cosine,
            OnDisk = true,
            Size = (ulong)dimensions
        };

        // 벡터 설정은 컬렉션 생성 이후 수정이 불가능.
        var config = new VectorParamsMap { Map = { [DefaultVectorsName] = param } };

        // 컬렉션 생성
        await _client.CreateCollectionAsync(
            collectionName: collectionName,
            vectorsConfig: config,
            cancellationToken: cancellationToken);

        // 인덱스 생성
        await _client.CreatePayloadIndexAsync(
            collectionName: collectionName,
            "lastUpdatedAt",
            schemaType: PayloadSchemaType.Integer,
            cancellationToken: cancellationToken);

        await _client.CreatePayloadIndexAsync(
            collectionName: collectionName,
            "vectorId",
            schemaType: PayloadSchemaType.Keyword,
            cancellationToken: cancellationToken);

        await _client.CreatePayloadIndexAsync(
            collectionName: collectionName,
            "sourceId",
            schemaType: PayloadSchemaType.Keyword,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (!await CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} is not exist");

        // 인덱스 삭제
        await _client.DeletePayloadIndexAsync(
            collectionName: collectionName,
            "lastUpdatedAt",
            cancellationToken: cancellationToken);

        await _client.DeletePayloadIndexAsync(
            collectionName: collectionName,
            "vectorId",
            cancellationToken: cancellationToken);

        await _client.DeletePayloadIndexAsync(
            collectionName: collectionName,
            "sourceId",
            cancellationToken: cancellationToken);

        // 컬렉션 삭제
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
            filter: condition,
            limit: (uint)limit,
            payloadSelector: true,
            vectorsSelector: true,
            orderBy: orderBy,
            cancellationToken: cancellationToken);

        var records = new List<VectorRecord>();
        foreach (var point in response.Result)
        {
            var vectors = point.Vectors.Vectors_.Vectors[DefaultVectorsName].Data.ToArray();
            var record = ConvertRecordToPayload(point.Payload);
            records.Add(new VectorRecord
            {
                Id = record.Id,
                Vectors = vectors,
                Source = record.Source,
                Content = record.Content,
                LastUpdatedAt = record.LastUpdatedAt,
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
            record.LastUpdatedAt = DateTime.UtcNow;
            var payload = ConvertRecordToPayload(record);

            points.Add(new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = new Dictionary<string, float[]>
                {
                    [DefaultVectorsName] = record.Vectors.ToArray(),
                },
                Payload =
                {
                    ["vectorId"] = payload["vectorId"],
                    ["sourceId"] = payload["sourceId"],
                    ["source"] = payload["source"],
                    ["content"] = payload["content"],
                    ["lastUpdatedAt"] = payload["lastUpdatedAt"],
                },
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
            var record = ConvertRecordToPayload(point.Payload);

            records.Add(new ScoredVectorRecord
            {
                VectorId = record.Id,
                Score = point.Score,
                Source = record.Source,
                Content = record.Content,
                LastUpdatedAt = record.LastUpdatedAt,
            });
        }

        return records;
    }

    // 필터링 조건을 생성합니다.
    private static Filter? BuildFilter(VectorRecordFilter? filter)
    {
        if (filter == null || !filter.Any())
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

    // VectorRecord를 Qdrant에서 사용하는 Payload로 변환합니다.
    private static MapField<string, Value> ConvertRecordToPayload(VectorRecord record)
    {
        var payload = new MapField<string, Value>
        {
            ["vectorId"] = record.Id,
            ["sourceId"] = record.Source.Id,
            ["source"] = JsonSerializer.Serialize(record.Source, JsonDefaultOptions.Options),
            ["content"] = JsonSerializer.Serialize(record.Content, JsonDefaultOptions.Options),
            ["lastUpdatedAt"] = new DateTimeOffset(record.LastUpdatedAt).ToUnixTimeMilliseconds(),
        };
        return payload;
    }

    // Qdrant에서 반환된 Point의 Payload를 VectorRecord로 변환합니다.
    private static VectorRecord ConvertRecordToPayload(MapField<string, Value> payload)
    {
        var vectorId = payload.GetValueOrDefault("vectorId")?.StringValue ?? string.Empty;
        var source = JsonSerializer.Deserialize<IMemorySource>(
            payload.GetValueOrDefault("source")?.StringValue ?? string.Empty)
            ?? throw new InvalidOperationException("source is not found");
        var content = JsonSerializer.Deserialize<object>(
            payload.GetValueOrDefault("content")?.StringValue ?? string.Empty);
        var lastUpdatedAt = DateTimeOffset.FromUnixTimeMilliseconds(
            payload.GetValueOrDefault("lastUpdatedAt")?.IntegerValue ?? 0).UtcDateTime;

        return new VectorRecord
        {
            Id = vectorId,
            Vectors = Array.Empty<float>(),
            Source = source,
            Content = content,
            LastUpdatedAt = lastUpdatedAt
        };
    }

    // QdrantClient 인스턴스를 생성합니다.
    private static QdrantClient CreateQdrantClient(QdrantConfig config)
    {
        return new QdrantClient(
            host: config.Host,
            port: config.Port,
            https: config.Https,
            apiKey: config.ApiKey,
            grpcTimeout: config.GrpcTimeout);
    }

    // 컬렉션 이름의 유효성 검사
    // Qdrant 규칙:
    // 1. 영문자, 숫자, 한글, 공백, 허용
    // 2. 대소문자 구분
    // 3. 1~255자 제한
    // 4. 특수문자 일부 제한(*, <, >, /, :, ...etc)
    private static string EnsureCollectionName(string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException("collection name is required", nameof(collectionName));

        if (collectionName.Length < 1 || collectionName.Length > 255)
            throw new ArgumentException("collection name must be from 1 to 255 characters", nameof(collectionName));

        // 이외 유효성 검사는 Qdrant에서 수행
        return collectionName;
    }
}
