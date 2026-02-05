# Multi-Agent Cycle 1: Orchestration Framework Baseline Tests

## 범위

Mock Agent 기반 오케스트레이션 프레임워크 기초 테스트. 모든 오케스트레이터 타입(Sequential, Parallel, Graph)과 횡단 기능(Checkpointing, HITL, TypedPipeline, Streaming, Error Handling)의 핵심 동작을 검증.

### 프로젝트 철학 정렬
- **빌드타임 안전성**: 모든 오케스트레이터 옵션이 타입 안전한 클래스로 정의, generic TypedPipeline이 컴파일타임 타입 체크 제공
- **Provider 추상화**: IAgent 인터페이스를 MockAgent로 구현하여 LLM 의존성 없이 오케스트레이션 로직만 격리 테스트
- **SDK 사용자 가치**: 오케스트레이터의 올바른 동작을 실증하여 사용자 신뢰 확보

## SDK 수정 사항

없음. 기존 오케스트레이션 프레임워크가 28개 시나리오 모두 정상 동작.

## 테스트 프레임워크 구성

### MockAgent
- `IAgent` 인터페이스의 완전한 구현
- `ResponseFunc` (동기) / `ResponseFuncAsync` (비동기) 주입으로 동작 커스터마이징
- `InvokeAsync`: 텍스트 응답을 `MessageResponse`로 래핑하여 반환
- `InvokeStreamingAsync`: Begin → Delta chunks → Done 라이프사이클 시뮬레이션
- API 키/LLM 호출 불필요 — 순수 in-process 테스트

### SimpleExecutor
- `ITypedExecutor<TInput, TOutput>`의 경량 구현
- 순수 함수를 TypedPipeline에서 사용하기 위한 래퍼

## 시나리오 현황

### 28 PASS / 0 FAIL

| # | 카테고리 | 시나리오 | 검증 대상 |
|---|----------|----------|-----------|
| 1 | sequential | basic-chain | 2 에이전트 체인, output→input 전달 |
| 2 | sequential | accumulate-history | AccumulateHistory=true 메시지 누적 |
| 3 | sequential | ordering | 3 에이전트 순차 실행 순서 보장 |
| 4 | parallel | basic-parallel | 3 에이전트 병렬 실행, 모두 성공 |
| 5 | parallel | merge-aggregation | ParallelResultAggregation.Merge 결과 병합 |
| 6 | parallel | max-concurrency | MaxConcurrency=2 동시성 제한 |
| 7 | graph | linear-abc | A→B→C 선형 그래프 토폴로지 순서 |
| 8 | graph | diamond-fan | A→{B,C}→D 다이아몬드 Fan-Out/Fan-In |
| 9 | graph | conditional-edge | 엣지 조건 false → 노드 스킵 |
| 10 | graph | cycle-detection | 순환 그래프 → InvalidOperationException |
| 11 | graph | builder-validation | 필수 필드 누락 → InvalidOperationException |
| 12 | checkpoint | store-crud | InMemoryCheckpointStore Save/Load/Delete |
| 13 | checkpoint | seq-checkpoint-cleanup | 성공 완료 시 체크포인트 삭제 확인 |
| 14 | checkpoint | checkpoint-on-failure | 실패 시 체크포인트 잔존 확인 |
| 15 | hitl | approval-granted | 모든 에이전트 승인 → 전체 실행 |
| 16 | hitl | approval-denied | 특정 에이전트 거부 → 오케스트레이션 중단 |
| 17 | hitl | selective-approval | RequireApprovalForAgents 선택적 승인 |
| 18 | typed-pipeline | string-int-chain | string→int→string 2단계 파이프라인 |
| 19 | typed-pipeline | three-stage | 3단계 체인 정확성 |
| 20 | typed-pipeline | agent-executor | AgentExecutor IAgent 래핑 |
| 21 | streaming | seq-streaming-events | Sequential 스트리밍 이벤트 라이프사이클 |
| 22 | streaming | parallel-streaming-events | Parallel 스트리밍 이벤트 |
| 23 | streaming | graph-streaming-events | Graph 스트리밍 이벤트 |
| 24 | error | no-agents | 에이전트 미등록 에러 핸들링 |
| 25 | error | stop-on-failure | StopOnAgentFailure=true 즉시 중단 |
| 26 | error | continue-on-failure | StopOnAgentFailure=false 계속 진행 |
| 27 | error | parallel-all-fail | 전체 실패 시 에러 메시지 집계 |
| 28 | error | hubspoke-no-hub | Hub 미설정 에러 핸들링 |

## 검증

```
# 빌드: 0 Error
dotnet build IronHive.slnx

# 단위 테스트: 134 PASS
dotnet test IronHive.slnx

# 오케스트레이션 테스트: 28 PASS / 0 FAIL
tests/multi-agent-test/run.ps1
```

## 변경된 파일

| 파일 | 작업 |
|------|------|
| `tests/multi-agent-test/Program.cs` | MockAgent, 28개 시나리오, TestResult 프레임워크 전체 구현 |
| `tests/multi-agent-test/run.ps1` | .env 의존성 제거, mock agent 전용 스크립트로 업데이트 |

## 도출된 SDK 이슈

### 관찰 사항 (수정 불필요)
| 관찰 | 설명 |
|------|------|
| 모든 오케스트레이터 정상 동작 | 28개 시나리오에서 SDK 수정 필요 없음 |
| GraphOrchestrator 순환 감지 정확 | Kahn 알고리즘 기반 빌드타임 검증 동작 |
| 체크포인트 라이프사이클 올바름 | 성공 시 삭제, 실패 시 보존 |
| HITL 선택적 승인 정상 | RequireApprovalForAgents HashSet 필터링 정확 |
| TypedPipeline 타입 안전성 확인 | 컴파일타임에 타입 불일치 감지 |

## 커버리지 분석

| 기능 영역 | 커버리지 | 미검증 |
|-----------|----------|--------|
| SequentialOrchestrator | 높음 | 체크포인트 재개 (resume) |
| ParallelOrchestrator | 높음 | FirstSuccess/Fastest aggregation |
| HubSpokeOrchestrator | 에러만 | Hub-Spoke 전체 라운드 동작 |
| GraphOrchestrator | 높음 | 대규모 DAG (10+ 노드) |
| Checkpointing | 중간 | 재개 후 스킵 동작 |
| HITL | 높음 | 스트리밍 + 승인 조합 |
| TypedPipeline | 높음 | 에러 전파 |
| Streaming | 높음 | MessageDelta 상세 내용 |

## 다음 사이클

- [ ] HubSpoke 전체 라운드 동작 (Hub JSON 파싱 → Spoke 위임 → 결과 집약)
- [ ] 체크포인트 재개 테스트 (실패 지점에서 재실행 시 스킵 확인)
- [ ] ParallelResultAggregation.FirstSuccess / Fastest
- [ ] 대규모 그래프 (Fan-Out 3+ 노드, 다중 레이어)
- [ ] 타임아웃 테스트 (AgentTimeout / Timeout)
- [ ] 스트리밍 + HITL 조합
- [ ] 실제 LLM Provider 통합 오케스트레이션 테스트
