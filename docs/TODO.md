# TODO LIST

### Providers
- OpenAI SDK로 업그레이드 보류(Response API에 ExtraBody기능 추가 될때까지)
- Google의 ThinkSignature 처리방법 재확인(text or functioncall contents에 thinking_signature가 포함되어야 함, 공통 MessageGenerator의 요청객체의 signature 처리방법에 대한 변경 혹은 Interactive API등 다른 방법 고려 필요)

### Orchestration
- [x] Sequential, Parallel, Hub-Spoke 오케스트레이W터
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

