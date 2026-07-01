# 아키텍처

## 개요

IronHive는 **이름 기반 레지스트리(Named Registry) + Builder 패턴**을 중심으로 설계된 모듈형 .NET 10 AI 프레임워크입니다.

```
HiveServiceBuilder
    │
    ▼
IHiveService
    ├── Models       (IModelService)
    ├── Messages     (IMessageService)
    ├── Embeddings   (IEmbeddingService)
    ├── Images       (IImageService)
    ├── Videos       (IVideoService)
    ├── Audio        (IAudioService)
    ├── Files        (IFileStorageService)
    └── Memory       (IMemoryService)
```

---

## 레이어 구조

```
┌──────────────────────────────────────────────────────┐
│  Application (콘솔, ASP.NET Core, 기타)              │
├──────────────────────────────────────────────────────┤
│  IronHive.Core                                       │
│  ┌──────────────┬────────────────┬────────────────┐  │
│  │  Services    │  Agent         │  Microsoft/    │  │
│  │  - Message   │  - BasicAgent  │  Adapters      │  │
│  │  - Embedding │  - Middleware  │  (M.E.AI)      │  │
│  │  - Image     │  - Orch.       │                │  │
│  │  - Audio     │  - Workflow    │                │  │
│  │  - Video     │                │                │  │
│  │  - Model     │                │                │  │
│  └──────────────┴────────────────┴────────────────┘  │
│  ┌──────────────┬────────────────┬────────────────┐  │
│  │  Tools       │  Memory        │  Storages      │  │
│  │  - Function  │  - Collection  │  - Local File  │  │
│  │  - Collection│  - Worker      │  - Local Vec.  │  │
│  │              │  - Pipelines   │  - Local Queue │  │
│  └──────────────┴────────────────┴────────────────┘  │
├──────────────────────────────────────────────────────┤
│  IronHive.Abstractions (인터페이스 & DTO만)           │
├───────────┬─────────────┬──────────────┬─────────────┤
│ Providers │ Storages    │ Plugins      │             │
│ - OpenAI  │ - Qdrant    │ - MCP        │             │
│ - Anthrop.│ - Amazon    │ - OpenAPI    │             │
│ - GoogleAI│ - Azure     │              │             │
│ - OAI Compat│ - RabbitMQ│              │             │
└───────────┴─────────────┴──────────────┴─────────────┘
```

---

## 의존성 규칙

의존성은 항상 **위 → 아래** 방향입니다.

```
Application
    ↓
IronHive.Core
    ↓
IronHive.Abstractions   ← Providers, Storages, Plugins도 여기에 의존
```

- **Abstractions** — 인터페이스, DTO, enum만. 외부 패키지 의존 없음.
- **Core** — Abstractions 구현. OpenTelemetry, System.Text.Json 의존.
- **Providers** — Core + Abstractions 참조. 각 LLM SDK 의존.
- **Storages** — Core + Abstractions 참조. 각 저장소 SDK 의존.
- **Plugins** — Core + Abstractions 참조.

---

## 핵심 인터페이스

### AI 서비스

| 인터페이스 | 역할 |
|-----------|------|
| `IMessageGenerator` | 프로바이더 저수준 LLM 호출 (`GenerateMessageAsync`, `GenerateStreamingMessageAsync`, `CountTokensAsync`) |
| `IMessageService` | 애플리케이션 고수준 LLM 호출 (도구 실행 루프 포함) |
| `IEmbeddingGenerator` | 프로바이더 임베딩 생성 |
| `IEmbeddingService` | 애플리케이션 임베딩 서비스 |
| `IImageGenerator` | 이미지 생성/편집 |
| `IVideoGenerator` | 비디오 생성 |
| `IAudioProcessor` | TTS / STT |
| `IModelFinder` | 모델 목록 조회 |

### Agent / Orchestration

| 인터페이스/클래스 | 역할 |
|-----------------|------|
| `IAgent` | Provider + Model + Instructions + Tools를 묶는 에이전트 |
| `IAgentOrchestrator` | 멀티에이전트 실행 및 스트리밍 |
| `ICheckpointStore` | 오케스트레이션 상태 저장/재개 |
| `IContextScope` | 서브에이전트 메시지 범위 한정 |
| `IResultDistiller` | 결과 요약/압축 |
| `ITypedExecutor<TIn,TOut>` | 타입 안전 실행기 |
| `ISpeakerSelector` | GroupChat 발언자 선택 전략 |
| `ITerminationCondition` | GroupChat 종료 조건 |

### 스토리지

| 인터페이스 | 역할 |
|-----------|------|
| `IFileStorage` | 파일 업로드/다운로드/삭제 |
| `IVectorStorage` | 벡터 CRUD + 유사도 검색 |
| `IQueueStorage` | 메시지 큐 (enqueue/dequeue) |

### 도구

| 인터페이스 | 역할 |
|-----------|------|
| `ITool` | 도구 정의 (UniqueName, Description, Parameters, RequiresApproval) |
| `IToolCollection` | 등록된 도구 관리 및 실행 |

---

## 이름 기반 라우팅

모든 Generator/Storage는 문자열 키(이름)로 등록됩니다. 요청 시 `Provider` 필드로 어떤 구현을 사용할지 결정합니다:

```csharp
var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", openAIGenerator)
    .AddMessageGenerator("claude", anthropicGenerator)
    .AddVectorStorage("qdrant-prod", qdrantStorage)
    .AddVectorStorage("local", localStorage)
    .Build();

// 요청 시 이름으로 라우팅
var agent = hive.CreateAgentFrom(cfg => {
    cfg.Provider = "claude";  // AnthropicMessageGenerator 사용
    cfg.Model = "claude-3-5-sonnet-20241022";
});
```

---

## 오케스트레이션 아키텍처

```
OrchestratorBase (공통 기반)
├── ExecuteAgentAsync()          — 에이전트 실행 + OTEL 추적
├── ExecuteAgentStreamingAsync() — 스트리밍 실행
├── SaveCheckpointAsync()        — 체크포인트 저장
├── LoadCheckpointAsync()        — 체크포인트 로드
├── DeleteCheckpointAsync()      — 체크포인트 삭제 (완료 시)
├── CheckApprovalAsync()         — HITL 승인 확인
├── ExtractMessage()             — 응답에서 메시지 추출
└── AggregateTokenUsage()        — 토큰 사용량 집계
```

---

## RAG 파이프라인

```
문서/파일 → TextExtraction → TextChunking → CreateVectors → StoreVectors
                                                 ↑
                                        IEmbeddingGenerator
```

`MemoryWorker`가 큐 기반 비동기 ETL을 처리합니다:

```
IQueueStorage (입력) → Pipeline Steps → IVectorStorage (출력)
```

---

## Microsoft.Extensions.AI 호환

IronHive 구현체를 M.E.AI 생태계와 연동합니다:

```csharp
// IronHive → M.E.AI IChatClient
var chatClient = new ChatClientAdapter(hive.Messages, "openai", "gpt-4o");

// IronHive → M.E.AI IEmbeddingGenerator<string, Embedding<float>>
var embedder = new EmbeddingGeneratorAdapter(hive.Embeddings, "openai", "text-embedding-3-small");

// IronHive ITool → M.E.AI AITool
var aiTool = new AIToolAdapter(myTool);
```

---

## 관측성 (Observability)

`HiveTelemetry` 정적 클래스가 OpenTelemetry 추적을 제공합니다:

- `ActivitySource`: `"IronHive"` — 에이전트/오케스트레이션 단위 추적
- `Meter`: `"IronHive"` — 토큰 사용량, 실행 시간 메트릭
- GenAI Semantic Conventions 준수

---

## 프로젝트 레이아웃

```
src/
├── IronHive.Abstractions/           # 인터페이스, DTO, enum
├── IronHive.Core/                   # 핵심 구현
│   ├── Agent/                       # BasicAgent, 미들웨어, 오케스트레이터
│   │   └── Orchestration/           # 6개 오케스트레이터 + 빌더 + 체크포인트
│   ├── Files/Parsers/               # PDF, Word, Excel, PPT, Image 파서
│   ├── Memory/                      # MemoryService, Worker, 파이프라인
│   ├── Microsoft/                   # M.E.AI 어댑터
│   ├── Services/                    # 서비스 구현체
│   ├── Storages/                    # 로컬 파일/벡터/큐 구현체
│   ├── Tools/                       # FunctionTool, FunctionToolFactory
│   ├── Utilities/                   # HiveTelemetry, MessageSerializer 등
│   └── Workflow/                    # 워크플로우 엔진
├── IronHive.Providers.*/            # LLM 프로바이더
│   ├── IronHive.Providers.OpenAI
│   ├── IronHive.Providers.Anthropic
│   ├── IronHive.Providers.GoogleAI  # Gemini + Vertex AI
│   └── IronHive.Providers.OpenAI.Compatible  # Ollama, GPUStack 등
├── IronHive.Storages.*/             # 외부 스토리지
│   ├── IronHive.Storages.Qdrant
│   ├── IronHive.Storages.Amazon
│   ├── IronHive.Storages.Azure
│   └── IronHive.Storages.RabbitMQ
└── IronHive.Plugins.*/              # 플러그인
    ├── IronHive.Plugins.MCP
    └── IronHive.Plugins.OpenAPI
```
