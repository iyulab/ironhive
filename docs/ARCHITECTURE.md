# 아키텍처

IronHive의 전체 시스템 아키텍처와 모듈 간 관계를 설명합니다.

## 설계 원칙

- **인터페이스 기반 설계**: 모든 주요 컴포넌트는 인터페이스로 정의되어 구현체 교체가 용이
- **레지스트리 패턴**: 프로바이더와 스토리지를 이름 기반으로 동적 관리
- **파이프라인 패턴**: 메모리 처리를 단계별 파이프라인으로 구성
- **DI 컨테이너 통합**: .NET의 표준 의존성 주입 패턴 활용

## 레이어 구조

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│              (WebServer, ConsoleTest, etc.)                 │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     IronHive.Core                           │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐   │
│  │ HiveService │ │MessageService│ │   MemoryService     │   │
│  └─────────────┘ └─────────────┘ └─────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│    Providers    │ │    Storages     │ │     Plugins     │
│  ├─ OpenAI      │ │  ├─ Qdrant      │ │  ├─ MCP         │
│  ├─ Anthropic   │ │  ├─ Azure       │ │  └─ OpenAPI     │
│  ├─ GoogleAI    │ │  ├─ Amazon      │ │                 │
│  └─ Ollama      │ │  └─ RabbitMQ    │ │                 │
└─────────────────┘ └─────────────────┘ └─────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                  IronHive.Abstractions                      │
│         (인터페이스, 기본 타입, 공통 계약 정의)               │
└─────────────────────────────────────────────────────────────┘
```

## 의존성 방향

```
IronHive.Abstractions  ←─────────────────────────────────────┐
        ↑                                                     │
        │ (구현)                                              │
        │                                                     │
IronHive.Core ←──────────────────────────────────────────────┤
        ↑                                                     │
        │ (참조)                                              │
        │                                                     │
┌───────┴───────┬───────────────┬───────────────┐            │
│               │               │               │            │
Providers.*   Storages.*    Plugins.*      Applications ─────┘
```

모든 모듈은 `Abstractions`만 참조하여 서로 직접 의존하지 않습니다.

## 핵심 레지스트리

### Provider Registry

프로바이더별 생성기를 이름으로 관리합니다.

```
IProviderRegistry
├── MessageGenerators: Dictionary<string, IMessageGenerator>
├── EmbeddingGenerators: Dictionary<string, IEmbeddingGenerator>
└── ModelCatalogs: Dictionary<string, IModelCatalog>
```

### Storage Registry

스토리지 백엔드를 타입과 이름으로 관리합니다.

```
IStorageRegistry
├── FileStorages: Dictionary<string, IFileStorage>
├── VectorStorages: Dictionary<string, IVectorStorage>
└── QueueStorages: Dictionary<string, IQueueStorage>
```

## 데이터 흐름

### 메시지 생성 흐름

```
UserMessage
    │
    ▼
┌─────────────────────────────────────────────────────────┐
│                    MessageService                        │
│  1. 프로바이더 레지스트리에서 MessageGenerator 조회       │
│  2. MessageGenerationRequest 구성                        │
│  3. ToolCollection에서 사용 가능한 도구 필터링           │
│  4. MessageGenerator.GenerateMessageAsync() 호출         │
└─────────────────────────────────────────────────────────┘
    │
    ▼
┌─────────────────────────────────────────────────────────┐
│               Tool Execution Loop                        │
│  while (response has tool calls):                        │
│    1. 각 도구 병렬 실행 (최대 3개 동시)                  │
│    2. 결과를 메시지에 추가                               │
│    3. 다시 MessageGenerator 호출                         │
└─────────────────────────────────────────────────────────┘
    │
    ▼
AssistantMessage (텍스트, 도구 호출 결과, 토큰 사용량)
```

### RAG 메모리 처리 흐름

```
문서 업로드 (Queue Message)
    │
    ▼
┌─────────────────────────────────────────────────────────┐
│                    MemoryWorker                          │
│                  Pipeline Execution                      │
├─────────────────────────────────────────────────────────┤
│  Step 1: TextExtractionPipeline                         │
│          └─ FileDecoder로 텍스트 추출 (PDF, DOCX 등)    │
│                        │                                 │
│                        ▼                                 │
│  Step 2: TextChunkingPipeline                           │
│          └─ 텍스트를 청크로 분할                         │
│                        │                                 │
│                        ▼                                 │
│  Step 3: CreateVectorsPipeline                          │
│          └─ EmbeddingGenerator로 벡터 생성              │
│                        │                                 │
│                        ▼                                 │
│  Step 4: StoreVectorsPipeline                           │
│          └─ VectorStorage에 저장                        │
└─────────────────────────────────────────────────────────┘
    │
    ▼
Vector Collection (검색 가능)
```

### 벡터 검색 흐름

```
검색 쿼리
    │
    ▼
┌─────────────────────────────────────────────────────────┐
│                   MemoryService                          │
│  1. GetCollectionAsync()로 컬렉션 조회                   │
│  2. EmbeddingService로 쿼리 벡터화                       │
│  3. VectorStorage.SearchAsync() 호출                     │
└─────────────────────────────────────────────────────────┘
    │
    ▼
검색 결과 (점수, 페이로드, 메타데이터)
```

## 확장 포인트

| 확장 영역 | 인터페이스 | 용도 |
|----------|-----------|------|
| AI 프로바이더 | `IMessageGenerator`, `IEmbeddingGenerator` | 새로운 LLM 서비스 추가 |
| 파일 스토리지 | `IFileStorage` | 새로운 파일 저장소 추가 |
| 벡터 스토리지 | `IVectorStorage` | 새로운 벡터 DB 추가 |
| 큐 스토리지 | `IQueueStorage` | 새로운 메시지 큐 추가 |
| 도구 | `ITool` | 커스텀 도구 구현 |
| 파이프라인 | `IWorkflowStep<MemoryContext>` | 메모리 처리 단계 추가 |
| 파일 디코더 | `IFileDecoder` | 새로운 파일 형식 지원 |

## 동시성 모델

- **도구 실행**: 최대 3개의 도구가 동시에 실행됩니다
- **스트리밍**: 응답 생성 중 `IAsyncEnumerable`로 점진적 출력
- **메모리 워커**: 큐 메시지를 비동기로 소비하며 파이프라인 처리
- **취소 지원**: 모든 비동기 작업에 `CancellationToken` 전파
