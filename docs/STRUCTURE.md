# STRUCTURE

## Service Classification

### Services Container
- IProviderRegistry         => Singleton                                      => Required
  - IProvider[]
- IStorageRegistry          => Singleton                                      => Required
  - IStorage[]
- IToolCollection           => Singleton                                      => Required
  - ITool[]

### AI Services
- IModelCatalogService      => Singleton                                      => Required
  - IProviderRegistry
- IMessageService             => Singleton                                      => Required
  - IProviderRegistry
  - IToolCollection
- IEmbeddingService           => Singleton                                      => Required
  - IProviderRegistry

### File Services
- IFileStorageService               => Singleton                                      => Required
  - IStorageRegistry
- IFileExtractionService              => Singleton                                      => Required
  - IFileMediaTypeDetector
  - IFileDecoder[]
  
### Orchestration Services
- IAgentOrchestrator           => Transient (per-use)
  - OrchestratorBase (abstract)
    - SequentialOrchestrator   : 순차 실행, PassOutputAsInput, 체크포인트/승인 통합
    - ParallelOrchestrator     : 병렬 실행, 결과 집계 전략
    - HubSpokeOrchestrator     : Hub-Spoke 패턴, 다중 라운드
    - GraphOrchestrator        : DAG 기반, Fan-In/Fan-Out, 조건부 엣지
  - ICheckpointStore           : 오케스트레이션 상태 저장/재개
    - InMemoryCheckpointStore  : ConcurrentDictionary 기반 기본 구현
  - ITypedExecutor<TIn,TOut>   : 타입 안전 실행기
    - AgentExecutor<TIn,TOut>  : IAgent 래퍼
    - TypedPipelineBuilder     : 타입 안전 파이프라인 체이닝

### Memory Services
- IMemoryService              => Singleton                          => Required
  - IStorageRegistry
  - IEmbeddingService
- IMemoryWorkerService
  - IStorageRegistry