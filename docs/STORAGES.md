# 스토리지 백엔드

IronHive에서 지원하는 스토리지 타입별 구현과 설정 방법입니다.

## 스토리지 유형

| 인터페이스 | 용도 |
|-----------|------|
| `IFileStorage` | 파일 저장/조회/삭제 |
| `IVectorStorage` | 벡터 임베딩 저장 + 유사도 검색 |
| `IQueueStorage` | 메시지 큐 기반 비동기 처리 |

파일 스토리지는 `HiveServiceBuilder` 에 이름으로 등록하고 `hive.Files` 로 쓴다(`AddFileStorage(name, storage)` 또는 아래 패키지의
편의 메서드). 벡터·큐 스토리지는 빌더에 등록하지 않는다 — 스토리지를 직접 만들어 그 인스턴스를 쓴다. 문서 적재·검색
파이프라인이 필요하면 [FluxIndex](https://github.com/iyulab/FluxIndex) 를 쓴다.

---

## 파일 스토리지 (IFileStorage)

### Amazon S3

**패키지**: `IronHive.Storages.Amazon`

```csharp
builder.AddAmazonS3Storage("s3", new AmazonS3Config
{
    AccessKey = "AKIA...",
    SecretAccessKey = "...",
    RegionCode = "ap-northeast-2",
    BucketName = "my-bucket"
});
```

지원 기능: S3 객체 업로드/다운로드, 버킷 관리, 다중 리전

### Azure Blob Storage

**패키지**: `IronHive.Storages.Azure`

```csharp
builder.AddAzureBlobStorage("azure-blob", new AzureStorageConfig
{
    // AuthType 기본값 = ConnectionString
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;",
    StorageName = "documents"   // 컨테이너 이름
});
```

⚠️ **`AzureStorageConfig.StorageName`은 컨테이너(또는 공유) 이름**이고, `AddAzureBlobStorage`의 첫
인자(`"azure-blob"`)는 이 스토리지를 가리키는 **논리 이름**이다. 이름이 같고 뜻이 다르므로 둘을 바꿔
넣으면 엉뚱한 컨테이너를 만든다. `StorageName`은 필수이며 `Validate()`가 비어 있으면 던진다.

`AuthType`으로 인증 방식을 고른다 — `ConnectionString`(기본) · `AccountKey`(+`AccountName`) ·
`SASToken` · `AzureIdentity`(+`TokenCredential`, 기본 `DefaultAzureCredential`).

지원 기능: Blob 업로드/다운로드, 컨테이너 관리

### Azure File Share

**패키지**: `IronHive.Storages.Azure`

```csharp
builder.AddAzureFilesStorage("azure-files", new AzureStorageConfig
{
    ConnectionString = "...",
    StorageName = "myshare"   // 파일 공유 이름 — Blob과 같은 속성을 쓴다
});
```

지원 기능: 계층적 디렉토리 구조, SMB 프로토콜 호환

---

## 벡터 스토리지 (IVectorStorage)

### Qdrant

**패키지**: `IronHive.Storages.Qdrant`

```csharp
using var vectors = new QdrantVectorStorage(new QdrantConfig
{
    Host = "localhost",
    Port = 6334,      // gRPC 포트
    ApiKey = "..."    // 선택사항
});
```

지원 기능: 고성능 벡터 검색, 컬렉션 관리, 페이로드 필터링, 앨리어스 관리

### IVectorStorage 인터페이스

```csharp
public interface IVectorStorage : IDisposable
{
    Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(CancellationToken cancellationToken = default);
    Task<bool> CollectionExistsAsync(string collectionName, CancellationToken cancellationToken = default);
    Task<VectorCollectionInfo?> GetCollectionInfoAsync(string collectionName, CancellationToken cancellationToken = default);
    Task CreateCollectionAsync(VectorCollectionInfo collection, CancellationToken cancellationToken = default);
    Task DeleteCollectionAsync(string collectionName, CancellationToken cancellationToken = default);

    Task<IEnumerable<VectorRecord>> FindVectorsAsync(
        string collectionName, int limit = 20, VectorRecordFilter? filter = null, CancellationToken cancellationToken = default);
    Task UpsertVectorsAsync(
        string collectionName, IEnumerable<VectorRecord> vectors, CancellationToken cancellationToken = default);
    Task DeleteVectorsAsync(
        string collectionName, VectorRecordFilter filter, CancellationToken cancellationToken = default);

    Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(
        string collectionName,
        float[] vector,
        float minScore = 0.0f,
        int limit = 5,
        VectorRecordFilter? filter = null,
        CancellationToken cancellationToken = default);
}
```

### 직접 사용 예시

```csharp
IVectorStorage vectorStorage = qdrantStorage;   // 직접 만든 스토리지 인스턴스(예: QdrantVectorStorage)
float[] embeddingVector = await hive.Embeddings.EmbedAsync("openai", "text-embedding-3-small", "문서 내용...");

// 컬렉션 생성 — 이름과 차원, 그 벡터를 만든 임베딩 모델을 함께 기록한다
await vectorStorage.CreateCollectionAsync(new VectorCollectionInfo
{
    Name = "documents",
    Dimensions = 1536,
    EmbeddingProvider = "openai",
    EmbeddingModel = "text-embedding-3-small"
});

// 벡터 저장 — 원본 문서 id(SourceId)와 검색 결과에 함께 돌려받을 Payload
await vectorStorage.UpsertVectorsAsync("documents",
[
    new VectorRecord
    {
        VectorId = "doc-1#0",
        SourceId = "doc-1",
        Vectors = embeddingVector,
        Payload = new Dictionary<string, object?>
        {
            ["text"] = "문서 내용...",
            ["source"] = "manual.pdf",
        }
    }
]);

// 유사도 검색 — 점수가 붙은 레코드(ScoredVectorRecord)가 돌아온다
float[] queryVector = await hive.Embeddings.EmbedAsync("openai", "text-embedding-3-small", "검색어");
var results = await vectorStorage.SearchVectorsAsync("documents", queryVector, limit: 5);
foreach (var hit in results)
    Console.WriteLine($"{hit.SourceId} {hit.Score:F3}");
```

---

## 큐 스토리지 (IQueueStorage)

### RabbitMQ

**패키지**: `IronHive.Storages.RabbitMQ`

```csharp
using var queue = new RabbitMQueueStorage(new RabbitMQConfig
{
    Host = "localhost",       // 기본값 "localhost"
    Port = 5672,              // 기본값 5672
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
    QueueName = "tasks",          // 필수
    // SslEnabled = true,
    // MessageTTL = 3_600_000,    // ms
});
```

구성 요소:
- `RabbitMQueueStorage` — 큐 관리 및 메시지 발행
- `RabbitMQConsumer` — 메시지 소비자
- `RabbitMQMessage` — 메시지 래퍼

### IQueueStorage 인터페이스

```csharp
public interface IQueueStorage : IDisposable
{
    Task EnqueueAsync<T>(T message, CancellationToken cancellationToken = default);
    Task<IQueueMessage<T>?> DequeueAsync<T>(CancellationToken cancellationToken = default);
    Task<IQueueConsumer> CreateConsumerAsync<T>(Func<IQueueMessage<T>, Task> onReceived, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
```

메서드에는 큐 이름 인자가 없다 — 스토리지 하나가 큐 하나다(`RabbitMQConfig.QueueName`). 큐가 여럿이면 스토리지를 큐마다 하나씩 만든다.

### 큐 사용 예시

```csharp
// 메시지 발행 — 메시지는 직렬화 가능한 어떤 타입이든 된다(여기서는 문서 id 문자열)
await queueStorage.EnqueueAsync("doc-001");

// 소비자 생성 — 메시지가 도착할 때마다 콜백이 불린다
using var consumer = await queueStorage.CreateConsumerAsync<string>(async message =>
{
    Console.WriteLine(message.Body);   // 처리
    await message.CompleteAsync();      // 처리 완료
    // 또는: await message.RequeueAsync();          // 재큐잉
    // 또는: await message.DeadAsync("reason");     // 폐기
});
await consumer.StartAsync(cancellationToken);
// ...
await consumer.StopAsync();
```

---

## 다중 파일 스토리지 구성

여러 파일 스토리지를 용도별로 등록하여 이름으로 구분합니다:

```csharp
var hive = new HiveServiceBuilder()
    .AddAmazonS3Storage("docs", s3Config)
    .AddAzureBlobStorage("archive", new AzureStorageConfig
    {
        ConnectionString = "...",
        StorageName = "archive"
    })
    .Build();

await hive.Files.WriteFileAsync("archive", "2026/report.pdf", pdfStream);
```

---

## 스토리지 선택 가이드

### 파일 스토리지

| 시나리오 | 추천 |
|----------|------|
| AWS 환경 | Amazon S3 |
| Azure 환경 | Azure Blob Storage |
| SMB 호환 공유 | Azure File Share |

### 벡터 스토리지

| 시나리오 | 추천 |
|----------|------|
| 벡터 저장소만 필요 | Qdrant (`IronHive.Storages.Qdrant`) |
| 문서 적재·하이브리드 검색 | FluxIndex |
| 대규모 / 고가용성 | Qdrant 클러스터 |

### 큐 스토리지

| 시나리오 | 추천 |
|----------|------|
| 분산 처리 | RabbitMQ |
| 고가용성 | RabbitMQ 클러스터 |

---

## 관련 문서

- [SETUP.md](SETUP.md) — 설정 가이드
