# Architecture

## Overview

IronHive는 **Registry + Builder** 패턴을 중심으로 설계된 모듈형 AI 프레임워크입니다. `HiveServiceBuilder`로 Provider, Storage, Tool을 등록하고, `IHiveService`를 통해 통합 접근합니다.

```
HiveServiceBuilder
    │
    ▼
IHiveService ──┬── IProviderRegistry  (LLM providers)
               ├── IStorageRegistry   (File, Queue, Vector)
               └── IToolCollection    (Function tools)
```

## Layer Architecture

```
┌──────────────────────────────────────────────────┐
│  Application                                     │
│  (ConsoleTest, WebServer, your app)              │
├──────────────────────────────────────────────────┤
│  IronHive.Core                                   │
│  ┌──────────────┬────────────┬──────────────┐    │
│  │  Services    │  Agent     │  Compatibility│    │
│  │  - Message   │  - Agent   │  - M.E.AI    │    │
│  │  - Embedding │  - Orch.   │    Adapters  │    │
│  │  - File      │            │              │    │
│  │  - Memory    │            │              │    │
│  └──────────────┴────────────┴──────────────┘    │
│  ┌──────────────┬────────────┬──────────────┐    │
│  │  Tools       │  Storages  │  Telemetry   │    │
│  │  - Function  │  - Local   │  - OTEL      │    │
│  │  - Pipeline  │    Vector  │  - Metrics   │    │
│  └──────────────┴────────────┴──────────────┘    │
├──────────────────────────────────────────────────┤
│  IronHive.Abstractions                           │
│  (interfaces only, zero implementation)          │
├────────────┬────────────┬────────────────────────┤
│ Providers  │ Storages   │ Plugins                │
│ - OpenAI   │ - Qdrant   │ - MCP                  │
│ - Anthropic│ - Amazon   │ - OpenAPI              │
│ - GoogleAI │ - Azure    │                        │
│ - Ollama   │ - RabbitMQ │                        │
└────────────┴────────────┴────────────────────────┘
```

## Dependency Rule

모든 의존성은 **위→아래** 방향입니다. Abstractions는 어디에도 의존하지 않습니다.

```
Providers / Storages / Plugins
        ↓
    IronHive.Core
        ↓
IronHive.Abstractions (interfaces only)
```

- **Abstractions** — 인터페이스, DTO, enum만 정의. 외부 패키지 의존 없음.
- **Core** — Abstractions 구현. OpenTelemetry, Polly, System.Text.Json 의존.
- **Providers** — Core + Abstractions 참조. 각 LLM SDK 의존.
- **Storages** — Core + Abstractions 참조. 각 저장소 SDK 의존.
- **Plugins** — Core + Abstractions 참조.

## Key Interfaces

### AI Services

| Interface | Responsibility |
|-----------|---------------|
| `IMessageGenerator` | LLM 텍스트 생성 (동기 + 스트리밍) |
| `IEmbeddingGenerator` | 텍스트 임베딩 벡터 생성 |
| `IModelCatalog` | 사용 가능한 모델 목록 조회 |
| `IAgent` | Provider + Model + SystemPrompt + Tools를 묶는 에이전트 |

### Orchestration

| Interface / Class | Responsibility |
|-------------------|---------------|
| `IAgentOrchestrator` | 멀티에이전트 실행 및 스트리밍 |
| `SequentialOrchestrator` | 순차 체인 실행. 출력→입력 전달 또는 히스토리 누적 |
| `ParallelOrchestrator` | 병렬 실행. 결과 집계 (All, FirstSuccess, Fastest, Merge) |
| `HubSpokeOrchestrator` | 중앙 Hub가 Spoke에 작업 위임. 다중 라운드 |
| `GraphOrchestrator` | DAG 기반 토폴로지 실행. 조건부 엣지, Fan-In/Fan-Out |
| `ICheckpointStore` | 오케스트레이션 상태 저장/재개 |
| `ITypedExecutor<TIn,TOut>` | 타입 안전 실행기 인터페이스 |

### Storage

| Interface | Responsibility |
|-----------|---------------|
| `IFileStorage` | 파일 업로드/다운로드/삭제 |
| `IQueueStorage` | 메시지 큐 (enqueue/dequeue) |
| `IVectorStorage` | 벡터 CRUD + 유사도 검색 |

### Tools

| Interface | Responsibility |
|-----------|---------------|
| `ITool` | 도구 정의 (이름, 설명, JSON Schema) |
| `IToolCollection` | 등록된 도구 관리 및 실행 |

## Orchestration Architecture

### Execution Model

모든 오케스트레이터는 `OrchestratorBase`를 상속하며, 공통 기능을 제공합니다:

```
OrchestratorBase (abstract)
├── ExecuteAgentAsync()         — 개별 에이전트 실행 + OTEL 추적
├── SaveCheckpointAsync()       — 체크포인트 저장
├── LoadCheckpointAsync()       — 체크포인트 로드 (재개)
├── CheckApprovalAsync()        — HITL 승인 확인
├── ExtractMessage()            — 응답에서 메시지 추출
└── AggregateTokenUsage()       — 토큰 사용량 집계
```

### Orchestrator Options Hierarchy

```
OrchestratorOptions (base)
├── Name, Timeout, AgentTimeout, StopOnAgentFailure
├── CheckpointStore, OrchestrationId
├── ApprovalHandler, RequireApprovalForAgents
│
├── SequentialOrchestratorOptions
│   └── PassOutputAsInput, AccumulateHistory
├── ParallelOrchestratorOptions
│   └── MaxConcurrency, ResultAggregation
├── HubSpokeOrchestratorOptions
│   └── MaxRounds, ParallelSpokes, MaxConcurrentSpokes
└── GraphOrchestratorOptions
```

### Graph Orchestrator

`GraphOrchestratorBuilder`로 DAG를 구성하고, Kahn 알고리즘으로 토폴로지 정렬 후 실행합니다.

```
Builder API:
    AddNode("id", agent)
    AddEdge("from", "to", condition?)
    SetStartNode("id")
    SetOutputNode("id")
    Build()  → 순환 감지 → GraphOrchestrator

Execution:
    1. Kahn's algorithm → topological order
    2. 각 노드: 인입 엣지 조건 확인 → 승인 체크 → 에이전트 실행
    3. Fan-In: 모든 인입 엣지의 소스 완료 시 실행
    4. Fan-Out: 출력 엣지가 여럿이면 이후 단계에서 분기
    5. 출력 노드의 결과가 최종 출력
```

### Typed Pipeline

`ITypedExecutor<TIn,TOut>`를 체이닝하여 컴파일타임 타입 안전성을 보장합니다.

```csharp
var pipeline = TypedPipeline
    .Start(executor1)       // ITypedExecutor<string, Analysis>
    .Then(executor2)        // ITypedExecutor<Analysis, Summary>
    .Build();               // ITypedExecutor<string, Summary>

var result = await pipeline.ExecuteAsync("input");
```

`AgentExecutor<TIn,TOut>`는 `IAgent`를 `ITypedExecutor`로 래핑합니다.

## RAG Pipeline

```
Document → TextExtraction → TextChunking → Embedding → VectorStorage
                                              ↑
                                       IEmbeddingGenerator
```

`MemoryWorker`가 큐 기반 비동기 ETL을 처리합니다:

```
IQueueStorage (input) → Pipeline Steps → IVectorStorage (output)
```

## Extension Patterns

### Provider 추가

각 Provider는 다음 인터페이스를 구현합니다:

```
IModelCatalog         — 모델 목록
IMessageGenerator     — 텍스트 생성
IEmbeddingGenerator   — 임베딩 생성 (선택)
*Config class         — API 키, 엔드포인트 등
```

### DelegatingMessageGenerator

`DelegatingMessageGenerator`를 상속하여 횡단 관심사를 미들웨어처럼 적용합니다:

```
Request → [Resilience] → [Telemetry] → [Actual Provider]
```

### M.E.AI Compatibility

`ChatClientAdapter`와 `EmbeddingGeneratorAdapter`로 `Microsoft.Extensions.AI` 생태계와 양방향 연동합니다.

## Observability

`IronHiveTelemetry` 정적 클래스가 OpenTelemetry 추적을 제공합니다:

- `ActivitySource`: "IronHive" — 에이전트/오케스트레이션 단위 추적
- `Meter`: "IronHive" — 토큰 사용량, 실행 시간 메트릭
- GenAI Semantic Conventions 준수

## Project Layout

```
src/
├── IronHive.Abstractions/      # Interfaces, DTOs, enums
├── IronHive.Core/              # Core implementation
│   ├── Agent/Orchestration/    # Orchestrators, Graph, Checkpoint, TypedPipeline
│   ├── Compatibility/          # M.E.AI adapters
│   ├── Services/               # MessageService, EmbeddingService, etc.
│   ├── Telemetry/              # OpenTelemetry integration
│   └── Tools/                  # Function tool system
├── IronHive.Providers.*/       # LLM providers (OpenAI, Anthropic, GoogleAI, Ollama)
├── IronHive.Storages.*/        # Storage backends (Qdrant, Amazon, Azure, RabbitMQ)
└── IronHive.Plugins.*/         # Plugins (MCP, OpenAPI)

tests/
└── IronHive.Tests/             # Unit tests (134 tests)
├── chat-api-tests/             # Provider integration tests (20 scenarios × 5 providers)
└── multi-agent-test/           # Orchestration tests (28 scenarios, mock agents)
```
