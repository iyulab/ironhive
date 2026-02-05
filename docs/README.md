# IronHive

.NET 10.0 기반의 AI/LLM 프레임워크로, RAG(Retrieval-Augmented Generation) 및 Agent 기반 애플리케이션 구축을 위한 모듈형 플러그인 아키텍처를 제공합니다.

## 핵심 기능

- **Multi-Provider 지원**: OpenAI, Anthropic, Google AI, Ollama 등 다양한 AI 프로바이더를 단일 인터페이스로 사용
- **유연한 스토리지**: Azure Blob, Amazon S3, Qdrant Vector DB, RabbitMQ 등 다양한 백엔드 지원
- **RAG 파이프라인**: 문서 추출 → 청킹 → 임베딩 → 벡터 저장의 완전한 메모리 처리 워크플로우
- **Agent 시스템**: 도구 호출이 포함된 멀티턴 대화 및 자동화 에이전트
- **플러그인 확장**: MCP(Model Context Protocol) 및 OpenAPI 기반 도구 통합

## 프로젝트 구조

```
IronHive/
├── src/
│   ├── IronHive.Abstractions   # 인터페이스 및 기본 타입 정의
│   ├── IronHive.Core           # 핵심 구현체
│   ├── IronHive.Providers.*    # AI 프로바이더 (4종)
│   ├── IronHive.Storages.*     # 스토리지 백엔드 (4종)
│   └── IronHive.Plugins.*      # 플러그인 시스템 (2종)
├── services/
│   └── WebServer               # 웹 서비스 예제
└── experiments/
    └── ConsoleTest             # 테스트 콘솔 앱
```

## 빠른 시작

### 1. HiveService 구성

```csharp
using IronHive.Core;

var builder = new HiveServiceBuilder();

// 프로바이더 등록
builder
    .AddMessageGenerator("openai", new OpenAIChatMessageGenerator(apiKey))
    .AddEmbeddingGenerator("openai", new OpenAIEmbeddingGenerator(apiKey));

// 스토리지 등록
builder
    .AddVectorStorage("qdrant", new QdrantVectorStorage(config));

// 도구 등록
builder.AddTool(FunctionToolFactory.Create<MyToolClass>());

// 서비스 빌드
IHiveService hive = builder.Build();
```

### 2. 기본 채팅

```csharp
var agent = new BasicAgent(hive.Services.GetRequiredService<IMessageService>())
{
    Provider = "openai",
    Model = "gpt-4o-mini",
    Name = "ChatBot"
};

var messages = new List<Message>
{
    new UserMessage("안녕하세요, 오늘 날씨가 어떤가요?")
};

await foreach (var response in agent.InvokeAsync(messages))
{
    Console.Write(response.Content);
}
```

### 3. RAG 검색

```csharp
// 벡터 컬렉션 가져오기
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");

// 유사도 검색
var results = await collection.SearchAsync("검색 쿼리", limit: 5);

foreach (var result in results)
{
    Console.WriteLine($"Score: {result.Score}, Text: {result.Payload["text"]}");
}
```

### 4. 메모리 워커 (백그라운드 처리)

```csharp
var worker = hive.CreateMemoryWorkerFrom(builder =>
{
    builder
        .UseStorage("rabbitmq")
        .UsePipeline(p => p
            .AddStep<TextExtractionPipeline>()
            .AddStep<TextChunkingPipeline>()
            .AddStep<CreateVectorsPipeline>()
            .AddStep<StoreVectorsPipeline>());
});

await worker.StartAsync();
```

## 패키지 구성

| 패키지 | 설명 |
|--------|------|
| `IronHive.Abstractions` | 핵심 인터페이스 및 추상화 계층 |
| `IronHive.Core` | 기본 구현체 및 오케스트레이션 |
| `IronHive.Providers.OpenAI` | OpenAI GPT 시리즈 지원 |
| `IronHive.Providers.Anthropic` | Anthropic Claude 시리즈 지원 |
| `IronHive.Providers.GoogleAI` | Google Gemini 시리즈 지원 |
| `IronHive.Providers.Ollama` | 로컬 Ollama 모델 지원 |
| `IronHive.Storages.Qdrant` | Qdrant 벡터 데이터베이스 |
| `IronHive.Storages.Azure` | Azure Blob/File Storage |
| `IronHive.Storages.Amazon` | Amazon S3 |
| `IronHive.Storages.RabbitMQ` | RabbitMQ 메시지 큐 |
| `IronHive.Plugins.MCP` | Model Context Protocol |
| `IronHive.Plugins.OpenAPI` | OpenAPI/Swagger 도구 통합 |

## 상세 문서

### 아키텍처
- [ARCHITECTURE.md](ARCHITECTURE.md) - 아키텍처 설계 및 모듈 구조
- [CORE-COMPONENTS.md](CORE-COMPONENTS.md) - 핵심 클래스 및 서비스

### 에이전트 시스템
- [AGENTS.md](AGENTS.md) - 에이전트 시스템 개요
- [ORCHESTRATION.md](ORCHESTRATION.md) - 멀티에이전트 오케스트레이션 패턴
- [MIDDLEWARE.md](MIDDLEWARE.md) - 미들웨어 시스템 (재시도, 타임아웃, 회로 차단기 등)

### 확장 기능
- [PROVIDERS.md](PROVIDERS.md) - AI 프로바이더 상세
- [STORAGES.md](STORAGES.md) - 스토리지 백엔드 상세
- [PLUGINS.md](PLUGINS.md) - 플러그인 시스템

## 요구 사항

- .NET 10.0 이상
- 각 프로바이더/스토리지에 대한 API 키 또는 접속 정보
