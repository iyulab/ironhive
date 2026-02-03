- builder
	- providers
	- storages
	- tools

	- AddModelCatalogProvider()
	- AddEmbeddingGenerator()
	- AddMessageGenerator()

	- AddFileStorage()
	- AddQueueStorage()
	- AddVectorStorage()

	- AddTool()

- service
	- Providers
	- Storages
	- Tools

	- CreateAgent()
	- CreateMemory()
	- CreateMemoryWorker()

## Orchestration

### Patterns
- **Sequential**: 순차 실행. 이전 출력 → 다음 입력. 히스토리 누적 옵션.
- **Parallel**: 동일 입력으로 병렬 실행. 결과 집계 전략 (All, FirstSuccess, Fastest, Merge).
- **Hub-Spoke**: 중앙 Hub 에이전트가 Spoke 에이전트를 조율. 다중 라운드.
- **Graph (DAG)**: 토폴로지 순서 실행. 조건부 엣지, Fan-In (인입 엣지 전체 완료 후 실행), Fan-Out (출력 엣지 병렬 분기).

### Cross-cutting Concerns
- **Checkpointing**: ICheckpointStore를 통해 오케스트레이션 중간 상태 저장/재개. 실패 시 완료된 단계부터 재시작 가능.
- **Human-in-the-Loop**: ApprovalHandler + RequireApprovalForAgents로 특정 에이전트 실행 전 승인 대기. 거부 시 체크포인트 저장 후 중단.
- **Observability**: IronHiveTelemetry.StartOrchestrationActivity()로 오케스트레이션 단위 OpenTelemetry 추적.
- **Typed Executor**: ITypedExecutor<TIn,TOut> + TypedPipeline으로 컴파일타임 타입 안전성 보장.
