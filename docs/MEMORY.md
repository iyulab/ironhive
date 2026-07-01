# RAG 메모리 파이프라인

벡터 기반 RAG(Retrieval-Augmented Generation) 시스템에 대한 상세 문서입니다.

## 개요

```
문서/파일 → TextExtraction → TextChunking → CreateVectors → StoreVectors
                                                 ↑
                                     IEmbeddingGenerator
                                         ↓
                                    IVectorStorage
```

`MemoryWorker`가 큐 기반으로 비동기 배경 처리를 담당합니다.

---

## 컬렉션 관리

### 컬렉션 생성

```csharp
// 새 컬렉션 생성
await hive.Memory.CreateCollectionAsync(
    storageName: "qdrant",          // 벡터 스토리지 이름
    collectionName: "documents",    // 컬렉션 이름
    embeddingProvider: "openai",    // 임베딩 프로바이더
    embeddingModel: "text-embedding-3-small"
);

// 컬렉션 존재 여부 확인
bool exists = await hive.Memory.CollectionExistsAsync("qdrant", "documents");

// 컬렉션 목록 조회
var collections = await hive.Memory.ListCollectionsAsync("qdrant", prefix: "docs-");

// 컬렉션 삭제
await hive.Memory.DeleteCollectionAsync("qdrant", "documents");
```

---

## MemoryWorker 설정

`IHiveService.CreateMemoryWorkerFrom()`으로 워커를 구성합니다:

```csharp
var worker = hive.CreateMemoryWorkerFrom(builder =>
    builder
        .UseQueue("local-queue")                     // 작업 큐
        .Then<TextExtractionPipeline>("extract")     // 텍스트 추출
        .Then<TextChunkingPipeline, TextChunkingOptions>("chunk",
            new TextChunkingOptions
            {
                ChunkSize = 512,   // 청크 크기 (토큰 기준)
                ChunkOverlap = 50  // 겹침 크기
            })
        .Then<CreateVectorsPipeline>("embed")        // 임베딩 생성
        .Then<StoreVectorsPipeline>("store")         // 벡터 저장
        .Build());

// 워커 시작
await worker.StartAsync(cancellationToken);

// 처리 완료/오류 이벤트 구독
worker.Progressed += (_, args) =>
{
    Console.WriteLine($"[{args.PipelineName}] {args.Status}: {args.Context.SourceId}");
};

// 워커 정지 (graceful)
await worker.StopAsync(force: false);  // 현재 작업 완료 후 정지
await worker.StopAsync(force: true);   // 즉시 정지
```

---

## 문서 인제스션

### IndexSource로 문서 큐 등록

```csharp
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");

// 파일 경로로 인덱싱
await collection.IndexSourceAsync("local-queue", new FileMemorySource
{
    SourceId = "doc-001",
    FilePath = "./documents/manual.pdf"
});

// 텍스트 직접 인덱싱
await collection.IndexSourceAsync("local-queue", new TextMemorySource
{
    SourceId = "text-001",
    Text = "IronHive는 .NET 10 AI 프레임워크입니다.",
    Metadata = new Dictionary<string, object>
    {
        ["category"] = "introduction",
        ["author"] = "team"
    }
});
```

### 소스 제거

```csharp
await collection.DeindexSourceAsync("doc-001");
```

---

## 의미적 검색

```csharp
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");

// 기본 검색
var results = await collection.SemanticSearchAsync("인공지능의 역사");

// 옵션 지정 검색
var results = await collection.SemanticSearchAsync(
    "machine learning applications",
    new SearchOptions
    {
        TopK = 10,           // 반환 결과 수 (기본값: 5)
        MinScore = 0.7f,     // 최소 유사도 점수 (0~1)
    });

foreach (var hit in results.Hits)
{
    Console.WriteLine($"[{hit.Score:F3}] {hit.Record.Content}");
    Console.WriteLine($"  Source: {hit.Record.Metadata?["source"]}");
}
```

---

## 기본 제공 파이프라인

| 파이프라인 | 설명 |
|-----------|------|
| `TextExtractionPipeline` | 파일에서 텍스트 추출 (PDF, DOCX, XLSX, PPTX, 이미지) |
| `TextChunkingPipeline` | 텍스트를 청크로 분할 (의미 경계 기반: 문단→문장→절→단어) |
| `CreateVectorsPipeline` | 임베딩 생성 |
| `StoreVectorsPipeline` | 벡터 저장소에 저장 |
| `DialogueExtractionPipeline` | 대화 형식 텍스트 추출 |

### 커스텀 파이프라인

```csharp
// IMemoryPipeline 구현
public class MyFilterPipeline : IMemoryPipeline
{
    public async Task<MemoryContext> ExecuteAsync(MemoryContext context, CancellationToken ct)
    {
        // 전처리 로직
        context.Text = context.Text?.ToUpper();
        return context;
    }
}

// 옵션이 있는 파이프라인
public class MyPipelineOptions { public int MaxLength { get; set; } = 1000; }

public class MyOptionsPipeline : IMemoryPipeline<MyPipelineOptions>
{
    public async Task<MemoryContext> ExecuteAsync(MemoryContext context, MyPipelineOptions options, CancellationToken ct)
    {
        // options.MaxLength 사용
        return context;
    }
}

// 등록
builder.Then<MyFilterPipeline>("filter")
       .Then<MyOptionsPipeline, MyPipelineOptions>("my-opts", new MyPipelineOptions { MaxLength = 500 });
```

---

## MemoryContext

```csharp
public class MemoryContext
{
    public string? SourceId { get; set; }           // 소스 고유 ID
    public string? StorageName { get; set; }        // 벡터 스토리지 이름
    public string? CollectionName { get; set; }     // 컬렉션 이름
    public string? EmbeddingProvider { get; set; }  // 임베딩 프로바이더
    public string? EmbeddingModel { get; set; }     // 임베딩 모델
    public string? FilePath { get; set; }           // 처리할 파일 경로
    public string? Text { get; set; }               // 추출된 텍스트
    public IEnumerable<VectorRecord>? Vectors { get; set; }  // 생성된 벡터
    public Dictionary<string, object>? Metadata { get; set; }
}
```

---

## RAG + 에이전트 통합

```csharp
// 1. 컬렉션에서 관련 문서 검색
var collection = await hive.Memory.GetCollectionAsync("qdrant", "docs");
var searchResults = await collection.SemanticSearchAsync("사용자 질문");

// 2. 검색 결과를 프롬프트에 포함
var context = string.Join("\n\n", searchResults.Hits.Select(h =>
    $"[소스: {h.Record.Metadata?["source"]}]\n{h.Record.Content}"));

// 3. 에이전트에 컨텍스트와 함께 질문 전달
var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider = "openai";
    cfg.Model = "gpt-4o";
    cfg.Instructions = $"다음 컨텍스트를 바탕으로 답변하세요:\n\n{context}";
});

var response = await agent.InvokeAsync("사용자 질문");
```

---

## ASP.NET Core에서 MemoryWorker

```csharp
// Program.cs
builder.Services.AddHostedService<MemoryWorkerHostedService>();

// MemoryWorkerHostedService.cs
public class MemoryWorkerHostedService(IHiveService hive) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var worker = hive.CreateMemoryWorkerFrom(builder =>
            builder
                .UseQueue("tasks")
                .Then<TextExtractionPipeline>("extract")
                .Then<TextChunkingPipeline, TextChunkingOptions>("chunk",
                    new TextChunkingOptions { ChunkSize = 512, ChunkOverlap = 50 })
                .Then<CreateVectorsPipeline>("embed")
                .Then<StoreVectorsPipeline>("store")
                .Build());

        await worker.StartAsync(stoppingToken);
    }
}
```

---

## 관련 문서

- [STORAGES.md](STORAGES.md) — 벡터/큐 스토리지 설정
- [PROVIDERS.md](PROVIDERS.md) — 임베딩 프로바이더
- [SERVICES.md](SERVICES.md) — IMemoryService 인터페이스
