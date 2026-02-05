# Multi-Agent Cycle 2: Advanced Orchestration Patterns

## 범위

MAF(Microsoft Agent Framework) 수준의 에이전트 오케스트레이션 기능 추가. 4개 Phase 구현:
1. **Composability**: 오케스트레이터를 IAgent로 래핑 (중첩 오케스트레이션)
2. **Handoff**: 에이전트 간 직접 제어권 위임 (Mesh 토폴로지)
3. **GroupChat**: 관리자 기반 반복 토론/정제 패턴
4. **Middleware**: 에이전트 실행 전후 인터셉션 체인

### 프로젝트 철학 정렬
- **빌드타임 안전성**: Builder.Build()에서 유효성 검증 실패 시 즉시 예외 (잘못된 handoff 대상, 누락된 speaker selector 등)
- **커스텀 자유도 최대화**: ISpeakerSelector, ITerminationCondition 인터페이스로 확장 가능, NoHandoffHandler로 HITL 지원
- **v0.x 과감한 설계**: IAgentMiddleware 체인 패턴, OrchestratorBase에 미들웨어 통합

## SDK 수정 사항

### Abstractions 변경

| 파일 | 변경 |
|------|------|
| `IAgentOrchestrator.cs` | `OrchestrationEventType`에 `Handoff`, `SpeakerSelected`, `HumanInputRequired` 추가 |
| `OrchestratorOptions.cs` | `HandoffOrchestratorOptions`, `GroupChatOrchestratorOptions` 클래스 추가; base에 `AgentMiddlewares` 프로퍼티 추가 |

### Abstractions 신규

| 파일 | 설명 |
|------|------|
| `HandoffTarget.cs` | Handoff 대상 정보 (AgentName, Description) |
| `ISpeakerSelector.cs` | GroupChat 발언자 선택 인터페이스 |
| `ITerminationCondition.cs` | GroupChat 종료 조건 인터페이스 |
| `IAgentMiddleware.cs` | 에이전트 미들웨어 인터페이스 |

### Core 변경

| 파일 | 변경 |
|------|------|
| `OrchestratorBase.cs` | `ExecuteAgentAsync`에 미들웨어 체인 지원 (`InvokeWithMiddlewaresAsync`) |

### Core 신규 (10개)

| 파일 | 설명 |
|------|------|
| `OrchestratorAgentAdapter.cs` | IAgentOrchestrator → IAgent 어댑터 |
| `OrchestratorExtensions.cs` | `.AsAgent()` 확장 메서드 |
| `HandoffOrchestrator.cs` | JSON 기반 handoff 감지, 시스템 프롬프트 주입 |
| `HandoffOrchestratorBuilder.cs` | Fluent builder, 빌드타임 유효성 검증 |
| `GroupChatOrchestrator.cs` | 공유 대화 히스토리, 발언자 선택 루프 |
| `GroupChatOrchestratorBuilder.cs` | Fluent builder, 편의 메서드 |
| `SpeakerSelectors.cs` | RoundRobin, Random, LLM 기반 선택기 |
| `TerminationConditions.cs` | MaxRounds, Keyword, TokenBudget, Composite 조건 |
| `MiddlewareAgent.cs` | IAgent 래퍼, 미들웨어 체인 실행 |
| `AgentExtensions.cs` | `.WithMiddleware()` 확장 메서드 |

## 시나리오 현황

### 45 PASS / 0 FAIL (기존 28 + 신규 17)

#### 기존 시나리오 (28개) — 회귀 없음

| # | 카테고리 | 시나리오 | 결과 |
|---|----------|----------|------|
| 1–3 | sequential | basic-chain, accumulate-history, ordering | PASS |
| 4–6 | parallel | basic-parallel, merge-aggregation, max-concurrency | PASS |
| 7–11 | graph | linear-abc, diamond-fan, conditional-edge, cycle-detection, builder-validation | PASS |
| 12–14 | checkpoint | store-crud, seq-checkpoint-cleanup, checkpoint-on-failure | PASS |
| 15–17 | hitl | approval-granted, approval-denied, selective-approval | PASS |
| 18–20 | typed-pipeline | string-int-chain, three-stage, agent-executor | PASS |
| 21–23 | streaming | seq/parallel/graph-streaming-events | PASS |
| 24–28 | error | no-agents, stop/continue-on-failure, parallel-all-fail, hubspoke-no-hub | PASS |

#### 신규 시나리오 (17개)

| # | 카테고리 | 시나리오 | 검증 대상 |
|---|----------|----------|-----------|
| 29 | composability | nested-orchestrator | Sequential을 Agent로 래핑 후 외부 Sequential에서 실행 |
| 30 | composability | streaming-forward | 중첩 오케스트레이터의 스트리밍 이벤트 forward |
| 31 | composability | failure-propagation | 내부 실패 → 외부 예외 전파 |
| 32 | handoff | basic-handoff | triage→billing→triage 3단 흐름 |
| 33 | handoff | max-transitions | MaxTransitions=3 초과 방지 |
| 34 | handoff | builder-validation | 존재하지 않는 대상 에이전트 참조 → 예외 |
| 35 | handoff | no-handoff-handler | NoHandoffHandler 콜백 호출 확인 |
| 36 | handoff | streaming-handoff | Handoff 이벤트 포함 전체 스트리밍 라이프사이클 |
| 37 | groupchat | roundrobin | RoundRobin 3에이전트 2라운드 순서 보장 |
| 38 | groupchat | keyword-termination | "DONE" 키워드 감지 → 종료 |
| 39 | groupchat | max-rounds | MaxRounds=5 안전 한도 |
| 40 | groupchat | composite-any | CompositeTermination(requireAll=false) 동작 |
| 41 | groupchat | streaming-speaker | SpeakerSelected 이벤트 수 검증 |
| 42 | middleware | chain-order | m1→m2→agent→m2→m1 체인 실행 순서 |
| 43 | middleware | short-circuit | next() 호출 없이 미들웨어에서 직접 반환 |
| 44 | middleware | orchestrator-middleware | OrchestratorOptions.AgentMiddlewares 전역 적용 |
| 45 | middleware | property-delegation | MiddlewareAgent가 inner 프로퍼티 위임 |

## 검증

```
# 빌드: 0 Error
dotnet build IronHive.slnx

# CI 테스트: 158 PASS (기존 133 + 신규 25)
dotnet test tests/IronHive.Tests/IronHive.Tests.csproj

# 로컬 테스트: 45 PASS / 0 FAIL (기존 28 + 신규 17)
dotnet run --project tests/multi-agent-test/MultiAgentTest.csproj
```

## CI 테스트 분포

| 테스트 파일 | 테스트 수 | Phase |
|-------------|----------|-------|
| OrchestratorAgentAdapterTests.cs | 4 | Phase 1 |
| HandoffOrchestratorTests.cs | 7 | Phase 2 |
| GroupChatOrchestratorTests.cs | 9 | Phase 3 |
| MiddlewareTests.cs | 5 | Phase 4 |
| **신규 합계** | **25** | |
| 기존 테스트 | 133 | Cycle 1 |
| **전체 합계** | **158** | |

## 도출된 SDK 이슈

### 관찰 사항 (수정 불필요)

| 관찰 | 설명 |
|------|------|
| HandoffOrchestrator JSON 파싱 안정 | GeneratedRegex 기반 `{"handoff_to": "..."}` 감지 정확 |
| SystemPrompt 임시 변경 패턴 안전 | try/finally로 원본 복원, 동시성 문제 없음 (단일 에이전트 실행) |
| GroupChat 종료 조건 조합 가능 | CompositeTermination으로 any/all 조합 정상 동작 |
| 미들웨어 체인 역순 구성 정확 | 등록 순서대로 실행, 역순 구성(reverse iteration) 올바름 |
| AsAgent 어댑터 투명하게 동작 | 중첩 오케스트레이션에서 내부/외부 구분 없이 실행 |

### 잠재적 개선점

| 항목 | 설명 | 심각도 |
|------|------|--------|
| HandoffOrchestrator SystemPrompt 동시성 | 동일 에이전트를 여러 오케스트레이터에서 공유하면 SystemPrompt 경쟁 상태 가능. 현재 단일 루프이므로 문제 없지만 병렬 사용 시 위험. | 낮음 |
| 스트리밍 미들웨어 미지원 | MiddlewareAgent.InvokeStreamingAsync는 미들웨어를 bypass. v0.x 제한으로 수용. | 낮음 |
| LlmSpeakerSelector 실제 LLM 미검증 | MockAgent 기반으로만 테스트. 실제 LLM의 이름 추출 정확도는 미확인. | 중간 |
| HandoffOrchestrator JSON 감지 범위 | `{"handoff_to": "..."}` 만 감지. 에이전트가 다른 JSON과 혼합하면 오탐 가능. | 낮음 |

## 커버리지 분석

| 기능 영역 | 커버리지 | 미검증 |
|-----------|----------|--------|
| OrchestratorAgentAdapter | 높음 | 실제 LLM 기반 중첩 |
| HandoffOrchestrator | 높음 | 다중 handoff 대상 선택 (3+ 에이전트), 컨텍스트 전달 내용 검증 |
| GroupChatOrchestrator | 높음 | LLM 기반 SpeakerSelector, TokenBudgetTermination |
| MiddlewareAgent | 높음 | 스트리밍 미들웨어, 오류 복구 미들웨어 |
| Builder 검증 | 높음 | 모든 빌더의 주요 유효성 검증 커버 |
| 스트리밍 | 높음 | 모든 오케스트레이터의 스트리밍 이벤트 검증 |

## 변경된 파일 전체 목록

### 신규 파일 (18개)

| 파일 | 위치 |
|------|------|
| `HandoffTarget.cs` | Abstractions/Agent/Orchestration/ |
| `ISpeakerSelector.cs` | Abstractions/Agent/Orchestration/ |
| `ITerminationCondition.cs` | Abstractions/Agent/Orchestration/ |
| `IAgentMiddleware.cs` | Abstractions/Agent/ |
| `OrchestratorAgentAdapter.cs` | Core/Agent/Orchestration/ |
| `OrchestratorExtensions.cs` | Core/Agent/Orchestration/ |
| `HandoffOrchestrator.cs` | Core/Agent/Orchestration/ |
| `HandoffOrchestratorBuilder.cs` | Core/Agent/Orchestration/ |
| `GroupChatOrchestrator.cs` | Core/Agent/Orchestration/ |
| `GroupChatOrchestratorBuilder.cs` | Core/Agent/Orchestration/ |
| `SpeakerSelectors.cs` | Core/Agent/Orchestration/ |
| `TerminationConditions.cs` | Core/Agent/Orchestration/ |
| `MiddlewareAgent.cs` | Core/Agent/ |
| `AgentExtensions.cs` | Core/Agent/ |
| `OrchestratorAgentAdapterTests.cs` | Tests/Agent/Orchestration/ |
| `HandoffOrchestratorTests.cs` | Tests/Agent/Orchestration/ |
| `GroupChatOrchestratorTests.cs` | Tests/Agent/Orchestration/ |
| `MiddlewareTests.cs` | Tests/Agent/Orchestration/ |

### 수정 파일 (4개)

| 파일 | 변경 내용 |
|------|----------|
| `IAgentOrchestrator.cs` | OrchestrationEventType enum 확장 (+3) |
| `OrchestratorOptions.cs` | HandoffOrchestratorOptions, GroupChatOrchestratorOptions, AgentMiddlewares 추가 |
| `OrchestratorBase.cs` | InvokeWithMiddlewaresAsync 미들웨어 체인 지원 |
| `tests/multi-agent-test/Program.cs` | 17개 신규 시나리오 + 미들웨어 헬퍼 클래스 |

## 다음 사이클

- [ ] 실제 LLM Provider 통합 오케스트레이션 테스트 (OpenAI/Anthropic)
- [ ] LlmSpeakerSelector 실제 LLM으로 검증
- [ ] HandoffOrchestrator 컨텍스트 전달 내용의 정확성 검증
- [ ] 스트리밍 미들웨어 지원 (v0.x 이후)
- [ ] RetryMiddleware, LoggingMiddleware 구현
- [ ] TokenBudgetTermination 실제 토큰 사용량 기반 테스트
- [ ] 대규모 GroupChat (5+ 에이전트, 20+ 라운드)
- [ ] Cycle 1 잔여: HubSpoke 전체 라운드, 체크포인트 재개, ParallelResultAggregation.FirstSuccess/Fastest
