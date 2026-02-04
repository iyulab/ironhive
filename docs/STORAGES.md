# 스토리지 백엔드

IronHive에서 지원하는 스토리지 타입별 구현과 설정 방법입니다.

## 스토리지 유형

IronHive는 세 가지 스토리지 인터페이스를 제공합니다:

| 인터페이스 | 용도 |
|-----------|------|
| `IFileStorage` | 파일 저장 및 조회 |
| `IVectorStorage` | 벡터 임베딩 저장 및 유사도 검색 |
| `IQueueStorage` | 메시지 큐 기반 비동기 처리 |

---

## 파일 스토리지

### Azure Blob Storage

**패키지**: `IronHive.Storages.Azure`

```csharp
builder.AddFileStorage("azure-blob", new AzureBlobFileStorage(new AzureStorageConfig
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;",
    ContainerName = "documents"
}));
```

**기능**:
- Blob 업로드/다운로드
- 컨테이너 관리
- SAS 토큰 지원

### Azure File Share

**패키지**: `IronHive.Storages.Azure`

```csharp
builder.AddFileStorage("azure-files", new AzureFileShareStorage(new AzureStorageConfig
{
    ConnectionString = "...",
    ShareName = "myshare"
}));
```

**기능**:
- 계층적 디렉토리 구조
- SMB 프로토콜 호환

### Amazon S3

**패키지**: `IronHive.Storages.Amazon`

```csharp
builder.AddFileStorage("s3", new AmazonS3FileStorage(new AmazonS3Config
{
    AccessKeyId = "AKIA...",
    SecretAccessKey = "...",
    Region = "ap-northeast-2",
    BucketName = "my-bucket"
}));
```

**기능**:
- S3 객체 업로드/다운로드
- 버킷 관리
- 다중 리전 지원

### 로컬 파일 시스템

**패키지**: `IronHive.Core` (기본 포함)

```csharp
builder.AddFileStorage("local", new LocalFileStorage(new LocalFileStorageConfig
{
    BasePath = "/data/files"
}));
```

**기능**:
- 로컬 디스크 파일 관리
- 개발/테스트 환경에 적합

---

## 벡터 스토리지

### Qdrant

**패키지**: `IronHive.Storages.Qdrant`

```csharp
builder.AddVectorStorage("qdrant", new QdrantVectorStorage(new QdrantConfig
{
    Host = "localhost",
    Port = 6334,
    ApiKey = "..."  // 선택사항
}));
```

**기능**:
- 고성능 벡터 검색
- 컬렉션 관리
- 필터링 검색
- 페이로드 저장
- 앨리어스 관리

**사용 예시**:

```csharp
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
        Vector = embeddings,
        Payload = new Dictionary<string, object>
        {
            ["text"] = "문서 내용...",
            ["source"] = "manual.pdf"
        }
    }
});

// 유사도 검색
var results = await vectorStorage.SearchVectorsAsync("documents", queryVector, limit: 5);
```

### 로컬 벡터 스토리지 (SQLite)

**패키지**: `IronHive.Core` (기본 포함)

```csharp
builder.AddVectorStorage("local", new LocalVectorStorage(new LocalVectorStorageConfig
{
    DatabasePath = "/data/vectors.db"
}));
```

**기능**:
- SQLite 기반 벡터 저장
- sqlite-vec 확장 사용
- 개발/소규모 환경에 적합
- 서버 설치 불필요

---

## 큐 스토리지

### RabbitMQ

**패키지**: `IronHive.Storages.RabbitMQ`

```csharp
builder.AddQueueStorage("rabbitmq", new RabbitMQueueStorage(new RabbitMQConfig
{
    HostName = "localhost",
    Port = 5672,
    UserName = "guest",
    Password = "guest",
    VirtualHost = "/"
}));
```

**구성 요소**:

| 클래스 | 설명 |
|--------|------|
| `RabbitMQueueStorage` | 큐 관리 및 메시지 발행 |
| `RabbitMQConsumer` | 메시지 소비자 |
| `RabbitMQMessage` | 메시지 래퍼 |

**사용 예시**:

```csharp
// 메시지 발행
await queueStorage.EnqueueAsync("memory-tasks", new MemoryContext
{
    FileId = "doc-123",
    CollectionName = "documents"
});

// 소비자 생성
var consumer = queueStorage.CreateConsumer<MemoryContext>("memory-tasks");
await foreach (var message in consumer.ConsumeAsync(cancellationToken))
{
    // 처리
    await message.CompleteAsync();  // 또는 RequeueAsync()
}
```

### 로컬 큐 (인메모리)

**패키지**: `IronHive.Core` (기본 포함)

```csharp
builder.AddQueueStorage("local", new LocalQueueStorage());
```

**기능**:
- SQLite 기반 영속 큐
- 개발/테스트 환경에 적합
- 단일 프로세스 환경

---

## 스토리지 선택 가이드

### 파일 스토리지

| 시나리오 | 추천 |
|----------|------|
| AWS 환경 | Amazon S3 |
| Azure 환경 | Azure Blob Storage |
| 온프레미스 | 로컬 파일 시스템 |
| 개발/테스트 | 로컬 파일 시스템 |

### 벡터 스토리지

| 시나리오 | 추천 |
|----------|------|
| 프로덕션 RAG | Qdrant |
| 개발/프로토타입 | 로컬 (SQLite) |
| 대규모 데이터 | Qdrant (클러스터) |

### 큐 스토리지

| 시나리오 | 추천 |
|----------|------|
| 분산 처리 | RabbitMQ |
| 단일 서버 | 로컬 큐 |
| 고가용성 | RabbitMQ (클러스터) |

---

## 다중 스토리지 구성

여러 스토리지를 동시에 등록하여 용도별로 사용할 수 있습니다:

```csharp
builder
    // 파일 스토리지 - 원본 문서는 S3, 임시 파일은 로컬
    .AddFileStorage("documents", new AmazonS3FileStorage(s3Config))
    .AddFileStorage("temp", new LocalFileStorage(localConfig))

    // 벡터 스토리지 - 프로덕션과 테스트 분리
    .AddVectorStorage("production", new QdrantVectorStorage(prodConfig))
    .AddVectorStorage("test", new LocalVectorStorage(testConfig))

    // 큐 스토리지
    .AddQueueStorage("tasks", new RabbitMQueueStorage(mqConfig));
```

사용 시 이름으로 구분:

```csharp
var prodVectors = hive.Storages.GetVectorStorage("production");
var testVectors = hive.Storages.GetVectorStorage("test");
```
