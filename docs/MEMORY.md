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
await worker.StartAsync();

// 진행/완료/오류 이벤트 구독 — WorkflowEventArgs<MemoryContext>
worker.Progressed += (_, args) =>
{
    // Type: Started | Progressed | Completed | Failed | Cancelled
    Console.WriteLine($"[{args.StepName}] {args.Type}: {args.Context.Source.Id}");
    if (args.Exception is not null)
        Console.WriteLine($"  {args.Message}");
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

// 파일 경로로 인덱싱 — StorageName은 파일을 읽어 올 파일 스토리지 이름이다
await collection.IndexSourceAsync("local-queue", new FileMemorySource
{
    Id = "doc-001",
    StorageName = "local-files",
    FilePath = "./documents/manual.pdf"
});

// 텍스트 직접 인덱싱
await collection.IndexSourceAsync("local-queue", new TextMemorySource
{
    Id = "text-001",
    Value = "IronHive는 .NET 10 AI 프레임워크입니다."
});
```

`Id`를 생략하면 GUID가 부여된다. 다만 `DeindexSourceAsync`가 이 값을 키로 쓰므로, 나중에 제거할
소스에는 직접 지정한다.

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
        Limit = 10,          // 반환 결과 수 (기본값: 5)
        MinScore = 0.7f,     // 최소 유사도 점수 (0~1)
        SourceIds = ["doc-001"],  // 특정 소스로 한정 (옵션)
    });

// VectorSearchResult { CollectionName, Query, Results }
// 각 항목은 ScoredVectorRecord — VectorRecord + Score
foreach (var hit in results.Results)
{
    Console.WriteLine($"[{hit.Score:F3}] {hit.SourceId} / {hit.VectorId}");
    if (hit.Payload.TryGetValue("text", out var text))
        Console.WriteLine($"  {text}");
}
```

레코드 본문은 별도 속성이 아니라 `Payload`에 담긴다(`IDictionary<string, object?>`, ordinal 비교).
**어떤 키가 들어가는지는 적재한 파이프라인이 정한다** — `CreateVectorsPipeline`은 청크 경로에서
`text`, 대화 경로(`DialogueExtractionPipeline`)에서 `question`·`answer`를 넣는다. 커스텀 파이프라인으로
적재했다면 그쪽이 정한 키를 읽는다.

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

파이프라인은 `MemoryContext`를 **제자리에서 수정하고** 성공/실패만 반환한다 — 컨텍스트를 반환값으로
넘기지 않는다. 단계 사이의 데이터는 `context.Payload`로 주고받는다(아래 `MemoryContext` 절 참조).

```csharp
// IMemoryPipeline 구현
public class MyFilterPipeline : IMemoryPipeline
{
    public Task<TaskStepResult> ExecuteAsync(MemoryContext context, CancellationToken ct = default)
    {
        // 앞 단계가 넣어 둔 값을 읽고, 바꿔서 다시 넣는다
        if (context.Payload.TryGetValue("text", out var value) && value is string text)
            context.Payload["text"] = text.ToUpperInvariant();

        return Task.FromResult(TaskStepResult.Success());
    }
}

// 옵션이 있는 파이프라인
public class MyPipelineOptions { public int MaxLength { get; set; } = 1000; }

public class MyOptionsPipeline : IMemoryPipeline<MyPipelineOptions>
{
    public Task<TaskStepResult> ExecuteAsync(
        MemoryContext context, MyPipelineOptions options, CancellationToken ct = default)
    {
        // options.MaxLength 사용
        return Task.FromResult(TaskStepResult.Success());
    }
}

// 등록
builder.Then<MyFilterPipeline>("filter")
       .Then<MyOptionsPipeline, MyPipelineOptions>("my-opts", new MyPipelineOptions { MaxLength = 500 });
```

실패는 예외를 던지거나 `TaskStepResult.Fail(exception)`을 반환한다. `TaskStepResult`는
`IsError`/`Message`/`Exception`을 가지며 `Success(message?)`·`Fail(exception)` 팩토리를 제공한다.

> 위 예제는 `tests/IronHive.Tests/Memory/DocumentedPipelineExampleTests.cs`에서 **컴파일·실행된다.**
> 이 절은 한때 인터페이스에 없던 시그니처(`Task<MemoryContext>` 반환)와 존재하지 않는 속성
> (`context.Text`)을 기술하고 있었다 — 산문은 그것을 알아채지 못하지만 컴파일러는 알아챈다.

---

## MemoryContext

컨텍스트는 **어디서 읽는지(`Source`)**, **어디로 쓰는지(`Target`)**, 그리고 **단계 사이에 주고받는
값(`Payload`)** 세 가지만 갖는다. 스토리지 이름·임베딩 모델처럼 대상에 따라 달라지는 설정은
컨텍스트에 평평하게 놓이지 않고 각 `IMemoryTarget` 구현이 소유한다.

```csharp
public class MemoryContext
{
    public required IMemorySource Source { get; set; }   // 무엇을 읽을지
    public required IMemoryTarget Target { get; set; }   // 어디에 적재할지
    public IDictionary<string, object?> Payload { get; } // 단계 간 전달값 (ordinal 비교)
}
```

### Source / Target

```csharp
public interface IMemorySource { string Id { get; set; } }   // "text" | "file" | "web"

public class TextMemorySource : MemorySourceBase { public required string Value { get; set; } }
public class FileMemorySource : MemorySourceBase
{
    public required string StorageName { get; set; }
    public required string FilePath { get; set; }
}

public interface IMemoryTarget { }                           // "vector"

public class VectorMemoryTarget : MemoryTargetBase
{
    public required string StorageName { get; set; }
    public required string CollectionName { get; set; }
    public required string EmbeddingProvider { get; set; }
    public required string EmbeddingModel { get; set; }
}
```

둘 다 `type` 판별자를 쓰는 다형 JSON 계약이므로 워커 정의를 직렬화해도 구현 타입이 보존된다.

파이프라인은 필요한 구현 타입으로 좁혀서 쓴다 — 좁혀지지 않으면 그 파이프라인이 이 대상에 대해
호출된 것 자체가 배선 오류다:

```csharp
if (context.Target is not VectorMemoryTarget target)
    throw new InvalidOperationException("target is not a VectorMemoryTarget");

// target.EmbeddingProvider / target.EmbeddingModel 사용
```

### Payload 키

기본 제공 파이프라인이 쓰는 키다. 커스텀 파이프라인을 중간에 끼울 때 이 이름으로 읽고 쓴다.

| 키 | 넣는 쪽 | 값 |
|---|---|---|
| `text` | `TextExtractionPipeline` | 추출된 원문 |
| `chunks` | `TextChunkingPipeline` · `DialogueExtractionPipeline` | 청크 또는 대화 목록 |
| `vectors` | `CreateVectorsPipeline` | 생성된 `VectorRecord` 목록 |

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
