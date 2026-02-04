# 핵심 컴포넌트

IronHive.Core의 주요 클래스와 서비스에 대한 상세 설명입니다.

## HiveService

프레임워크의 메인 파사드로, 모든 서비스에 대한 진입점입니다.

### 구성

```csharp
public interface IHiveService
{
    IServiceProvider Services { get; }      // DI 컨테이너
    IProviderRegistry Providers { get; }    // 프로바이더 레지스트리
    IStorageRegistry Storages { get; }      // 스토리지 레지스트리
    IToolCollection Tools { get; }          // 도구 컬렉션
    IMemoryService Memory { get; }          // 메모리 서비스
}
```

### HiveServiceBuilder

플루언트 API로 서비스를 구성합니다:

```csharp
var hive = new HiveServiceBuilder()
    // 프로바이더 등록
    .AddMessageGenerator("openai", generator)
    .AddEmbeddingGenerator("openai", embedder)
    .AddModelCatalog("openai", catalog)

    // 스토리지 등록
    .AddFileStorage("s3", fileStorage)
    .AddVectorStorage("qdrant", vectorStorage)
    .AddQueueStorage("rabbitmq", queueStorage)

    // 도구 등록
    .AddTool(tool)
    .AddTools(toolCollection)

    // 서비스 등록 (DI)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IMyService, MyService>();
    })

    .Build();
```

---

## MessageService

LLM과의 대화를 처리하고 도구 호출 루프를 관리합니다.

### 인터페이스

```csharp
public interface IMessageService
{
    IAsyncEnumerable<MessageResponse> GenerateMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default);
}
```

### MessageRequest

```csharp
var request = new MessageRequest
{
    Provider = "openai",           // 프로바이더 이름
    Model = "gpt-4o-mini",         // 모델 ID
    System = "You are helpful.",   // 시스템 프롬프트
    Messages = messages,           // 대화 메시지 목록
    Tools = ["search", "calc"],    // 사용할 도구 이름 (null = 전체)
    Options = new GenerationOptions
    {
        Temperature = 0.7f,
        MaxTokens = 4096
    }
};
```

### MessageResponse

```csharp
public class MessageResponse
{
    public string? Id { get; set; }           // 응답 ID
    public string? DoneReason { get; set; }   // 완료 이유 (stop, tool_use 등)
    public Message? Message { get; set; }      // 생성된 메시지
    public TokenUsage? Usage { get; set; }    // 토큰 사용량
}
```

### 도구 실행 루프

MessageService는 자동으로 도구 호출을 감지하고 실행합니다:

1. LLM이 `tool_use` 응답 생성
2. 해당 도구 찾아서 실행 (최대 3개 병렬)
3. 결과를 메시지에 추가
4. LLM에 다시 전송
5. `stop` 응답까지 반복

---

## Agent

에이전트는 MessageService를 래핑하여 특정 역할의 대화 인터페이스를 제공합니다.

### BasicAgent

```csharp
public class BasicAgent : IAgent
{
    public string? Name { get; set; }           // 에이전트 이름
    public string? Provider { get; set; }       // 프로바이더
    public string? Model { get; set; }          // 모델
    public string? Instruction { get; set; }    // 시스템 프롬프트
    public IEnumerable<string>? Tools { get; set; }  // 사용 도구
    public GenerationOptions? Options { get; set; }  // 생성 옵션
}
```

### 사용법

```csharp
var agent = new BasicAgent(messageService)
{
    Name = "Assistant",
    Provider = "openai",
    Model = "gpt-4o-mini",
    Instruction = "You are a helpful assistant.",
    Tools = ["web_search", "calculator"]
};

await foreach (var response in agent.InvokeAsync(messages))
{
    // 스트리밍 응답 처리
}
```

### AgentService

YAML/TOML/JSON 설정 파일에서 에이전트를 생성합니다:

```yaml
# agent.yaml
name: "Research Assistant"
provider: "anthropic"
model: "claude-3-sonnet"
instruction: |
  You are a research assistant.
  Always cite your sources.
tools:
  - web_search
  - document_reader
options:
  temperature: 0.3
  max_tokens: 8192
```

```csharp
var agentService = hive.Services.GetRequiredService<IAgentService>();
var agent = await agentService.LoadAgentAsync("agents/research.yaml");
```

---

## MemoryService

RAG를 위한 벡터 컬렉션 관리 서비스입니다.

### 인터페이스

```csharp
public interface IMemoryService
{
    Task<IMemoryCollection> GetCollectionAsync(
        string storageName,
        string collectionName,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<string>> ListCollectionsAsync(
        string storageName,
        CancellationToken cancellationToken = default);
}
```

### MemoryCollection

```csharp
public interface IMemoryCollection
{
    string Name { get; }
    VectorCollectionInfo Info { get; }

    Task<IEnumerable<VectorSearchResult>> SearchAsync(
        string query,
        int limit = 10,
        VectorFilter? filter = null,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        IEnumerable<VectorRecord> records,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        IEnumerable<string> ids,
        CancellationToken cancellationToken = default);
}
```

### 검색 예시

```csharp
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");

// 기본 검색
var results = await collection.SearchAsync("인공지능의 역사", limit: 5);

// 필터링 검색
var results = await collection.SearchAsync("인공지능", limit: 10, filter: new VectorFilter
{
    Field = "category",
    Operator = FilterOperator.Equals,
    Value = "technology"
});
```

---

## MemoryWorker

백그라운드에서 문서를 처리하여 벡터로 변환합니다.

### 파이프라인 구성

```csharp
var worker = hive.CreateMemoryWorkerFrom(builder =>
{
    builder
        .UseQueueStorage("rabbitmq", "memory-tasks")
        .UseVectorStorage("qdrant")
        .UseEmbedding("openai", "text-embedding-3-small")
        .UsePipeline(p => p
            .AddStep<TextExtractionPipeline>()
            .AddStep<TextChunkingPipeline>(new ChunkingOptions
            {
                ChunkSize = 512,
                Overlap = 50
            })
            .AddStep<CreateVectorsPipeline>()
            .AddStep<StoreVectorsPipeline>());
});
```

### 실행

```csharp
// 시작
await worker.StartAsync(cancellationToken);

// 종료 (graceful)
await worker.StopAsync(cancellationToken);
```

### 기본 제공 파이프라인

| 파이프라인 | 설명 |
|-----------|------|
| `TextExtractionPipeline` | 파일에서 텍스트 추출 (PDF, DOCX, PPT 등) |
| `TextChunkingPipeline` | 텍스트를 청크로 분할 |
| `CreateVectorsPipeline` | 임베딩 생성 |
| `StoreVectorsPipeline` | 벡터 저장소에 저장 |
| `DialogueExtractionPipeline` | 대화 형식 추출 |

---

## 파일 처리

### FileExtractionService

다양한 파일 형식에서 텍스트를 추출합니다.

```csharp
var extractor = hive.Services.GetRequiredService<IFileExtractionService>();
var text = await extractor.ExtractTextAsync(fileStream, "application/pdf");
```

### 지원 파일 디코더

| 디코더 | 지원 형식 |
|--------|----------|
| `PDFDecoder` | PDF (PdfPig 사용) |
| `WordDecoder` | DOCX (OpenXML 사용) |
| `PPTDecoder` | PPTX |
| `TextDecoder` | TXT, MD, 코드 파일 등 |
| `ImageDecoder` | PNG, JPG, GIF 등 |

### 커스텀 디코더 등록

```csharp
builder.ConfigureServices(services =>
{
    services.AddSingleton<IFileDecoder, MyCustomDecoder>();
});
```

---

## 메시지 타입

### Message 계층

```
Message (추상)
├── UserMessage        # 사용자 입력
├── AssistantMessage   # AI 응답
└── ToolMessage        # 도구 실행 결과
```

### 콘텐츠 타입

```
IContent (인터페이스)
├── TextContent        # 텍스트
├── ImageContent       # 이미지 (Base64 또는 URL)
├── ToolUseContent     # 도구 호출 요청
├── ToolResultContent  # 도구 실행 결과
└── ThinkingContent    # AI 사고 과정 (Anthropic)
```

### 메시지 구성 예시

```csharp
var messages = new List<Message>
{
    new UserMessage(new TextContent("이 이미지에 대해 설명해주세요")),
    new UserMessage(new ImageContent
    {
        MediaType = "image/png",
        Data = Convert.ToBase64String(imageBytes)
    })
};
```

---

## 직렬화

### MessageSerializer

메시지와 대화를 JSON으로 직렬화/역직렬화합니다.

```csharp
// 직렬화
var json = MessageSerializer.Serialize(messages);

// 역직렬화
var messages = MessageSerializer.Deserialize<List<Message>>(json);
```

다형성 타입을 자동으로 처리하며, `$type` 필드로 구체 타입을 구분합니다.

---

## 유틸리티

### TextCleaner

텍스트 정규화 및 정리:

```csharp
var cleaned = TextCleaner.Clean(rawText, new CleaningOptions
{
    RemoveExtraWhitespace = true,
    NormalizeLineEndings = true
});
```

### SqliteVecInstaller

SQLite 벡터 확장 자동 설치:

```csharp
await SqliteVecInstaller.EnsureInstalledAsync();
```
