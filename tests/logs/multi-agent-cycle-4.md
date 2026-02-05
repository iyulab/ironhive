# Multi-Agent Cycle 4: ISpeakerSelector 인터페이스 개선

## 범위

Cycle 3에서 도출된 P1 이슈 해결: `ISpeakerSelector`에 초기 메시지(conversationHistory) 전달 기능 추가

## SDK 변경 사항

### 인터페이스 변경 (Breaking Change)

| 파일 | 변경 |
|------|------|
| `ISpeakerSelector.cs` | `SelectNextSpeakerAsync`에 `conversationHistory` 매개변수 추가 |

**Before:**
```csharp
Task<string?> SelectNextSpeakerAsync(
    IReadOnlyList<AgentStepResult> steps,
    IReadOnlyList<IAgent> agents,
    CancellationToken cancellationToken = default);
```

**After:**
```csharp
Task<string?> SelectNextSpeakerAsync(
    IReadOnlyList<AgentStepResult> steps,
    IReadOnlyList<Message> conversationHistory,  // 추가
    IReadOnlyList<IAgent> agents,
    CancellationToken cancellationToken = default);
```

### 구현체 업데이트

| 파일 | 변경 |
|------|------|
| `SpeakerSelectors.cs` | `RoundRobinSpeakerSelector`, `RandomSpeakerSelector`, `LlmSpeakerSelector` 시그니처 업데이트 |
| `GroupChatOrchestrator.cs` | `SelectNextSpeakerAsync` 호출 시 `conversationHistory` 전달 |

### LlmSpeakerSelector 개선

1. **초기 메시지 맥락 포함**: 첫 번째 라운드에서도 user 메시지 맥락으로 에이전트 선택 가능
2. **에이전트 설명 포함**: 프롬프트에 각 에이전트의 `Description` 포함하여 선택 정확도 향상
3. **이름 매칭 완화**: 정확한 매칭 실패 시 부분 일치도 시도

## 테스트 결과

### 비교 (Cycle 3 → Cycle 4)

| 테스트 | Cycle 3 | Cycle 4 |
|--------|---------|---------|
| llm-handoff | PASS | PASS |
| llm-groupchat | PASS | PASS |
| llm-speaker-math | **FAIL** | **PASS** |
| llm-speaker-history | **FAIL** | PASS* |
| llm-streaming-handoff | **FAIL** | PASS* |

*LLM 테스트는 비결정적이므로 실행마다 결과 변동 가능

### 전체 요약

| 카테고리 | 결과 |
|----------|------|
| Mock 테스트 | 45 PASS |
| LLM 통합 테스트 | 4-5 PASS (변동) |
| **합계** | 49-50 PASS / 0-1 FAIL |

## 검증

```bash
# 빌드: 0 Error
dotnet build IronHive.slnx

# CI 테스트: 158 PASS
dotnet test tests/IronHive.Tests/IronHive.Tests.csproj

# 로컬 테스트: 49-50 PASS (LLM 변동성)
dotnet run --project tests/multi-agent-test/MultiAgentTest.csproj
```

## 변경된 파일

| 파일 | 변경 내용 |
|------|----------|
| `src/IronHive.Abstractions/Agent/Orchestration/ISpeakerSelector.cs` | 인터페이스 시그니처 변경 |
| `src/IronHive.Core/Agent/Orchestration/SpeakerSelectors.cs` | 3개 구현체 업데이트, LlmSpeakerSelector 개선 |
| `src/IronHive.Core/Agent/Orchestration/GroupChatOrchestrator.cs` | conversationHistory 전달 (2곳) |
| `tests/multi-agent-test/Program.cs` | 테스트 에러 메시지 개선 |

## 도출된 SDK 이슈

### 해결됨: P1 ISpeakerSelector 초기 메시지 미전달

✅ `conversationHistory` 매개변수 추가로 해결

### 잔여: P2 LLM Handoff JSON 출력 불확실성 (Medium)

LLM이 지시대로 JSON을 출력하지 않을 수 있음. 테스트 결과 변동의 주요 원인.

**관찰**:
- `llm-streaming-handoff` 테스트가 때때로 실패
- LLM이 인사말만 하고 handoff JSON 없이 응답하는 경우 발생

**가능한 개선**:
- Structured output 강제
- Few-shot examples 프롬프트에 포함
- 재시도 로직 추가

## 다음 사이클

- [ ] P2 Handoff JSON 감지 개선
- [ ] Middleware 확장 (RetryMiddleware, LoggingMiddleware)
- [ ] LLM 테스트 안정성 개선 (재시도, 더 명확한 프롬프트)
- [ ] Cycle 1/2 잔여 항목 처리

## 프로젝트 철학 정렬

- **v0.x 과감한 리팩토링**: `ISpeakerSelector` Breaking Change 적용 — 하위호환 불필요
- **빌드타임 안전성**: 인터페이스 변경으로 모든 구현체 컴파일 타임 검증
- **커스텀 자유도 최대화**: `conversationHistory` 접근으로 더 정교한 speaker 선택 가능
