using System.Text.Json;
using Google.Protobuf.Collections;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;
using IronHive.Abstractions.Json;
using IronHive.Abstractions.Vector;

namespace IronHive.Storages.Qdrant;

/// <summary>
/// Qdrant 벡터 스토리지 구현 클래스입니다.
/// </summary>
public class QdrantVectorStorage : IVectorStorage
{
    // Qdrant에서 사용할 기본 텍스트용 벡터의 이름입니다.
    private const string DefaultVectorsName = "text";

    private readonly QdrantClient _client;

    public QdrantVectorStorage(QdrantConfig config)
    {
        _client = CreateQdrantClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(
        CancellationToken cancellationToken = default)
    {
        var aliases = await _client.ListAliasesAsync(cancellationToken);
        var colls = aliases.Select(a => QdrantAliasHelper.Parse(a.AliasName))
            .DistinctBy(c => c.Name);
        return colls;
    }

    /// <inheritdoc />
    public async Task<bool> CollectionExistsAsync(
        string collectionName, 
        CancellationToken cancellationToken = default)
    {
        return await _client.CollectionExistsAsync(collectionName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<VectorCollectionInfo?> GetCollectionInfoAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        var aliases = await _client.ListCollectionAliasesAsync(collectionName, cancellationToken);
        var alias = aliases.ElementAtOrDefault(0);
        if (alias == null) return null;

        return QdrantAliasHelper.Parse(alias);
    }

    /// <inheritdoc />
    public async Task CreateCollectionAsync(
        VectorCollectionInfo collection,
        CancellationToken cancellationToken = default)
    {
        collection.Name = EnsureCollectionName(collection.Name);
        if (await _client.CollectionExistsAsync(collection.Name, cancellationToken))
            throw new InvalidOperationException($"collection {collection.Name} already exist");

        // 컬렉션 생성(벡터 설정은 컬렉션 생성 이후 수정 불가능)
        await _client.CreateCollectionAsync(
            collectionName: collection.Name,
            vectorsConfig: new VectorParamsMap 
            { 
                Map = 
                {
                    [DefaultVectorsName] = new VectorParams
                    {
                        Datatype = Datatype.Float32,
                        Distance = Distance.Cosine,
                        Size = (ulong)collection.Dimensions,
                        OnDisk = true,
                    }
                }
            },
            cancellationToken: cancellationToken);

        // 인덱스 생성
        await _client.CreatePayloadIndexAsync(
            collectionName: collection.Name,
            "lastUpsertedAt",
            schemaType: PayloadSchemaType.Integer,
            cancellationToken: cancellationToken);
        await _client.CreatePayloadIndexAsync(
            collectionName: collection.Name,
            "vectorId",
            schemaType: PayloadSchemaType.Keyword,
            cancellationToken: cancellationToken);
        await _client.CreatePayloadIndexAsync(
            collectionName: collection.Name,
            "sourceId",
            schemaType: PayloadSchemaType.Keyword,
            cancellationToken: cancellationToken);

        // Alias 생성
        var collectionAlias = QdrantAliasHelper.Create(collection);
        await _client.CreateAliasAsync(
            aliasName: collectionAlias,
            collectionName: collection.Name,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        if (!await _client.CollectionExistsAsync(collectionName, cancellationToken))
            throw new InvalidOperationException($"collection {collectionName} does not exist");

        // 인덱스 삭제
        await _client.DeletePayloadIndexAsync(
            collectionName: collectionName,
            "lastUpsertedAt",
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
        var orderBy = new OrderBy { Key = "lastUpsertedAt", Direction = Direction.Desc };
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
            //var vectors = point.Vectors.Vectors.Vectors[DefaultVectorsName].Data.ToArray();
            var record = ConvertPayloadToRecord(point.Payload);
            
            records.Add(new VectorRecord
            {
                VectorId = record.VectorId,
                SourceId = record.SourceId,
                Payload = record.Payload,
                LastUpsertedAt = record.LastUpsertedAt,
                Vectors = [] // 실제 벡터 데이터는 반환하지 않음
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
            var point = new PointStruct
            {
                //Id = Guid.NewGuid(),
                Vectors = new Dictionary<string, float[]>
                {
                    [DefaultVectorsName] = record.Vectors.ToArray(),
                }
            };
            point.Payload.Add("vectorId", record.VectorId);
            point.Payload.Add("sourceId", record.SourceId);
            point.Payload.Add("payload", JsonSerializer.Serialize(record.Payload, JsonDefaultOptions.Options));
            point.Payload.Add("lastUpsertedAt", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());

            points.Add(point);
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
        float[] vector,
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
            //var vectors = point.Vectors.Vectors.Vectors[DefaultVectorsName].Data.ToArray();
            var record = ConvertPayloadToRecord(point.Payload);
            
            records.Add(new ScoredVectorRecord
            {
                Score = point.Score,
                VectorId = record.VectorId,
                SourceId = record.SourceId,
                Payload = record.Payload,
                LastUpsertedAt = record.LastUpsertedAt,
                Vectors = [] // 실제 벡터 데이터는 반환하지 않음
            });
        }

        return records;
    }

    /// <summary>
    /// 필터링 조건을 생성합니다.
    /// </summary>
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

    /// <summary>
    /// Qdrant에서 반환된 Point의 Payload를 VectorRecord로 변환합니다.
    /// </summary>
    private static VectorRecord ConvertPayloadToRecord(MapField<string, Value> payload)
    {
        var vectorId = payload.GetValueOrDefault("vectorId")?.StringValue ?? string.Empty;
        var sourceId = payload.GetValueOrDefault("sourceId")?.StringValue ?? string.Empty;
        var payloadDict = JsonSerializer.Deserialize<IDictionary<string, object?>>(
            payload.GetValueOrDefault("payload")?.StringValue ?? "{}")
            ?? new Dictionary<string, object?>();
        var lastUpsertedAt = DateTimeOffset.FromUnixTimeMilliseconds(
            payload.GetValueOrDefault("lastUpsertedAt")?.IntegerValue ?? 0).UtcDateTime;

        return new VectorRecord
        {
            VectorId = vectorId,
            SourceId = sourceId,
            Vectors = [],
            Payload = payloadDict,
            LastUpsertedAt = lastUpsertedAt
        };
    }

    /// <summary>
    /// QdrantClient 인스턴스를 생성합니다.
    /// </summary>
    private static QdrantClient CreateQdrantClient(QdrantConfig config)
    {
        return new QdrantClient(
            host: config.Host,
            port: config.Port,
            https: config.Https,
            apiKey: config.ApiKey,
            grpcTimeout: config.GrpcTimeout);
    }

    /// <summary>
    /// Qdrant 컬렉션 이름의 유효성을 검사하고, 반환합니다.
    /// </summary>
    public static string EnsureCollectionName(string collectionName)
    {
        // Qdrant 규칙:
        // 1. 영문자, 숫자, 한글, 공백, 허용
        // 2. 대소문자 구분
        // 3. 1~255자 제한
        // 4. 특수문자 일부 제한(*, <, >, /, :, ...etc)
        if (string.IsNullOrWhiteSpace(collectionName))
            throw new ArgumentException("collection name is required", nameof(collectionName));

        if (collectionName.Length < 1 || collectionName.Length > 255)
            throw new ArgumentException("collection name is too long", nameof(collectionName));

        if (collectionName.StartsWith(QdrantAliasHelper.Prefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"collection name cannot start with '{QdrantAliasHelper.Prefix}'", nameof(collectionName));

        return collectionName;
    }
}
