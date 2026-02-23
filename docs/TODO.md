# TODO LIST

### Providers Migration
- [x] Anthropic & Google Provider => SDK로 변경(OpenAI는 보류)
- [x] OpenAI Provider AddProperties 삭제 => ExtraBody로 업그레이드(중복 프로퍼티들 가능하게.. 커스텀 어트리뷰트 생성)
- [x] Ollama Package 삭제 => OpenAI.Compatible로 이동(OpenAI API로 업그레이드 됨)
- Google의 ThinkSignature 처리방법이 특이함, text or functioncall에 넣어야 하므로, Interactive API등 다른 방법을 고려

### Agent
- 기능 설계

### Orchestration
- [x] Sequential, Parallel, Hub-Spoke 오케스트레이터
- [x] Graph(DAG) 오케스트레이터 (조건부 엣지, Fan-In/Fan-Out)
- [x] 체크포인팅 (ICheckpointStore, InMemoryCheckpointStore)
- [x] Human-in-the-Loop (ApprovalHandler, RequireApprovalForAgents)
- [x] 타입 안전 Executor (ITypedExecutor, AgentExecutor, TypedPipeline)
- [x] Observability 상수 정리 (StartOrchestrationActivity)
- [x] 파일 기반 CheckpointStore 구현 (JSON 직렬화)
- [x] ParallelOrchestrator 체크포인트/승인 통합
- [x] GraphOrchestrator Fan-Out 병렬 실행 + LoadCheckpoint 추가
- [x] HubSpokeOrchestrator 체크포인트/승인 통합
- [x] HandoffOrchestrator 체크포인트/승인 통합
- [x] GroupChatOrchestrator 체크포인트/승인 통합

### Providers
- ImageGenerator 기능 추가
- VideoGenerator 기능 추가
- AudioGenerator 기능 추가(TTS, STT?, Music)

#### Queue Storages
- QueueStorage에 Dequeue 또는 Consume 시 Hidden_Timeout 고려
- RabbitMQ, Azure ServiceBus, AWS SQS, Google Pub/Sub DeadLetter 지원 확인
