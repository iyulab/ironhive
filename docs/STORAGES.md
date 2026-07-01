# 스토리지 백엔드

IronHive에서 지원하는 스토리지 타입별 구현과 설정 방법입니다.

## 스토리지 유형

| 인터페이스 | 용도 |
|-----------|------|
| `IFileStorage` | 파일 저장/조회/삭제 |
| `IVectorStorage` | 벡터 임베딩 저장 + 유사도 검색 |
| `IQueueStorage` | 메시지 큐 기반 비동기 처리 |

---

## 파일 스토리지 (IFileStorage)

### 로컬 파일 시스템

**패키지**: `IronHive.Core` (기본 포함)

```csharp
// 편의 메서드 사용
builder.AddLocalFileStorage("local");

// 직접 등록
builder.AddFileStorage("local", new LocalFileStorage());
```

용도: 개발/테스트 환경, 단일 서버 운영

### Amazon S3

**패키지**: `IronHive.Storages.Amazon`

```csharp
builder.AddAmazonS3Storage("s3", new AmazonS3Config
{
    AccessKeyId = "AKIA...",
    SecretAccessKey = "...",
    Region = "ap-northeast-2",
    BucketName = "my-bucket"
});
```

지원 기능: S3 객체 업로드/다운로드, 버킷 관리, 다중 리전

### Azure Blob Storage

**패키지**: `IronHive.Storages.Azure`

```csharp
builder.AddAzureBlobStorage("azure-blob", new AzureStorageConfig
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;",
    ContainerName = "documents"
});
```

지원 기능: Blob 업로드/다운로드, 컨테이너 관리

### Azure File Share

**패키지**: `IronHive.Storages.Azure`

```csharp
builder.AddAzureFilesStorage("azure-files", new AzureStorageConfig
{
    ConnectionString = "...",
    ShareName = "myshare"
});
```

지원 기능: 계층적 디렉토리 구조, SMB 프로토콜 호환

---

## 벡터 스토리지 (IVectorStorage)

### 로컬 벡터 스토리지 (SQLite)

**패키지**: `IronHive.Core` (기본 포함)

```csharp
builder.AddLocalVectorStorage("local-vec", new LocalVectorConfig
{
    Path = "./data/vectors.db"
});
```

구현 방식: SQLite + sqlite-vec(vec0) 익스텐션. `SqliteVecInstaller`로 자동 설치.

용도: 개발/소규모 환경, 서버 설치 불필요

### Qdrant

**패키지**: `IronHive.Storages.Qdrant`

```csharp
builder.AddVectorStorage("qdrant", new QdrantVectorStorage(new QdrantConfig
{
    Host = "localhost",
    Port = 6334,      // gRPC 포트
    ApiKey = "..."    // 선택사항
}));
```

지원 기능: 고성능 벡터 검색, 컬렉션 관리, 페이로드 필터링, 앨리어스 관리

### IVectorStorage 인터페이스

```csharp
public interface IVectorStorage
{
    Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(string? prefix, CancellationToken ct);
    Task CreateCollectionAsync(string name, VectorCollectionInfo info, CancellationToken ct);
    Task DeleteCollectionAsync(string name, CancellationToken ct);

    Task UpsertVectorsAsync(string collection, IEnumerable<VectorRecord> records, CancellationToken ct);
    Task DeleteVectorsAsync(string collection, IEnumerable<string> ids, CancellationToken ct);
    Task<IEnumerable<VectorRecord>> FindVectorsAsync(string collection, IEnumerable<string> ids, CancellationToken ct);

    Task<VectorSearchResult> SearchVectorsAsync(
        string collection,
        ReadOnlyMemory<float> vector,
        int limit,
        VectorRecordFilter? filter = null,
        CancellationToken ct = default);
}
```

### 직접 사용 예시

```csharp
var vectorStorage = /* ... */;

// 컬렉션 생성
await vectorStorage.CreateCollectionAsync("documents", new VectorCollectionInfo
{
    Dimensions = 1536,
    EmbeddingProvider = "openai",
    EmbeddingModel = "text-embedding-3-small"
});

// 벡터 저장
await vectorStorage.UpsertVectorsAsync("documents", new[]
{
    new VectorRecord
    {
        Id = "doc-1",
        Vector = embeddingVector,
        Content = "문서 내용...",
        Metadata = new Dictionary<string, object>
        {
            ["source"] = "manual.pdf",
            ["category"] = "technology"
        }
    }
});

// 유사도 검색
var result = await vectorStorage.SearchVectorsAsync("documents", queryVector, limit: 5);
```

---

## 큐 스토리지 (IQueueStorage)

### 로컬 큐 스토리지

**패키지**: `IronHive.Core` (기본 포함)

```csharp
builder.AddLocalQueueStorage("local-queue", new LocalQueueConfig
{
    Path = "./data/queue"
});
```

구현 방식: 파일 시스템 기반 (`.qmsg`, `.qlock`, `.qdead` 확장자). 단일 프로세스 환경에 적합.

### RabbitMQ

**패키지**: `IronHive.Storages.RabbitMQ`

```csharp
builder.AddQueueStorage("rabbitmq", new RabbitMQueueStorage(new RabbitMQConfig
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/",
    QueueName = "memory-tasks"
}));
```

구성 요소:
- `RabbitMQueueStorage` — 큐 관리 및 메시지 발행
- `RabbitMQConsumer` — 메시지 소비자
- `RabbitMQMessage` — 메시지 래퍼

### IQueueStorage 인터페이스

```csharp
public interface IQueueStorage
{
    Task EnqueueAsync<T>(T message, string? queueName = null, CancellationToken ct = default);
    Task<T?> DequeueAsync<T>(string? queueName = null, CancellationToken ct = default);
    Task<IQueueConsumer<T>> CreateConsumerAsync<T>(string? queueName = null, CancellationToken ct = default);
    Task<long> CountAsync(string? queueName = null, CancellationToken ct = default);
    Task ClearAsync(string? queueName = null, CancellationToken ct = default);
}
```

### 큐 사용 예시

```csharp
// 메시지 발행
await queueStorage.EnqueueAsync(new MemoryContext
{
    StorageName = "qdrant",
    CollectionName = "documents",
    FilePath = "./doc.pdf"
});

// 소비자 생성
var consumer = await queueStorage.CreateConsumerAsync<MemoryContext>();
await foreach (var message in consumer.ConsumeAsync(cancellationToken))
{
    // 처리
    await ProcessAsync(message.Payload);
    await message.CompleteAsync();   // 처리 완료
    // 또는: await message.RequeueAsync();  // 재큐잉
}
```

---

## 다중 스토리지 구성

여러 스토리지를 용도별로 등록하여 이름으로 구분합니다:

```csharp
var hive = new HiveServiceBuilder()
    // 파일 스토리지
    .AddAmazonS3Storage("docs", s3Config)
    .AddLocalFileStorage("temp")

    // 벡터 스토리지
    .AddVectorStorage("prod-vec", qdrantStorage)
    .AddLocalVectorStorage("dev-vec", localVecConfig)

    // 큐 스토리지
    .AddQueueStorage("tasks", rabbitMQStorage)
    .AddLocalQueueStorage("dev-queue", localQueueConfig)

    .Build();
```

---

## 스토리지 선택 가이드

### 파일 스토리지

| 시나리오 | 추천 |
|----------|------|
| AWS 환경 | Amazon S3 |
| Azure 환경 | Azure Blob Storage |
| 온프레미스 / 개발 | 로컬 파일 시스템 |

### 벡터 스토리지

| 시나리오 | 추천 |
|----------|------|
| 프로덕션 RAG | Qdrant |
| 개발 / 프로토타이핑 | 로컬 SQLite |
| 대규모 / 고가용성 | Qdrant 클러스터 |

### 큐 스토리지

| 시나리오 | 추천 |
|----------|------|
| 분산 처리 | RabbitMQ |
| 단일 서버 / 개발 | 로컬 큐 |
| 고가용성 | RabbitMQ 클러스터 |

---

## 관련 문서

- [MEMORY.md](MEMORY.md) — RAG 메모리 파이프라인
- [SETUP.md](SETUP.md) — 설정 가이드
