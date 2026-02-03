# TODO LIST

## Agent
- 기능 설계
- 대화 기록 및 목록 혹은 세션 관리

## Orchestration
- [x] Sequential, Parallel, Hub-Spoke 오케스트레이터
- [x] Graph(DAG) 오케스트레이터 (조건부 엣지, Fan-In/Fan-Out)
- [x] 체크포인팅 (ICheckpointStore, InMemoryCheckpointStore)
- [x] Human-in-the-Loop (ApprovalHandler, RequireApprovalForAgents)
- [x] 타입 안전 Executor (ITypedExecutor, AgentExecutor, TypedPipeline)
- [x] Observability 상수 정리 (StartOrchestrationActivity)
- [ ] 파일 기반 CheckpointStore 구현 (JSON 직렬화)
- [ ] ParallelOrchestrator/HubSpokeOrchestrator에 체크포인트/승인 통합
- [ ] GraphOrchestrator Fan-Out 병렬 실행 최적화

## Providers
- GoogleAI 확장 추가
- XAI 확장추가

- ImageGenerator 기능 추가
- VideoGenerator 기능 추가
- AudioGenerator 기능 추가(TTS, STT?, Music))

## Plugins

## Storages

### Queue Storages
- QueueStorage에 Dequeue 또는 Consume 시 Hidden_Timeout 고려
- RabbitMQ, Azure ServiceBus, AWS SQS, Google Pub/Sub DeadLetter 지원 확인
