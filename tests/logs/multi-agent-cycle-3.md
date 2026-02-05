# Multi-Agent Cycle 3: Real LLM Integration Tests

## 범위

실제 LLM Provider (OpenAI/Anthropic)를 사용한 오케스트레이션 통합 테스트:
1. **Handoff**: 실제 LLM으로 에이전트 간 핸드오프 검증
2. **GroupChat**: 실제 LLM으로 GroupChat 패턴 검증
3. **LlmSpeakerSelector**: LLM 기반 발언자 선택기 검증
4. **Streaming Handoff**: 스트리밍 모드에서 핸드오프 이벤트 검증

## 테스트 결과

### 전체 요약

| 카테고리 | PASS | FAIL | SKIP |
|----------|------|------|------|
| Mock 테스트 (기존) | 45 | 0 | 0 |
| LLM 통합 테스트 | 2 | 3 | 0 |
| **합계** | **47** | **3** | **0** |

### LLM 통합 테스트 상세

| 시나리오 | 결과 | 설명 |
|----------|------|------|
| llm-handoff | PASS | Triage → Tech-support 핸드오프 성공, steps=2 |
| llm-groupchat | PASS | Writer + Critics 3에이전트 토론, rounds=4 |
| llm-speaker-math | FAIL | LlmSpeakerSelector가 math-expert 선택 실패 |
| llm-speaker-history | FAIL | LlmSpeakerSelector가 history-expert 선택 실패 |
| llm-streaming-handoff | FAIL | Handoff JSON 미감지 (LLM이 JSON 출력 안함) |

## 도출된 SDK 이슈

### P1: LlmSpeakerSelector 초기 메시지 미전달 (High)

**문제**: `ISpeakerSelector.SelectNextSpeakerAsync`는 `IReadOnlyList<AgentStepResult> steps`만 받음. 첫 번째 라운드에서 steps가 비어있어 초기 user 메시지 맥락 없이 에이전트 선택해야 함.

**영향**: LLM 기반 발언자 선택이 맥락 없이 작동하여 정확도 저하.

**제안 해결책**:
```csharp
// 옵션 A: 인터페이스 확장
Task<string?> SelectNextSpeakerAsync(
    IReadOnlyList<AgentStepResult> steps,
    IReadOnlyList<Message> conversationHistory,  // 추가
    IReadOnlyList<IAgent> agents,
    CancellationToken cancellationToken = default);

// 옵션 B: steps에 초기 메시지 포함된 가상 step 추가
```

### P2: LLM Handoff JSON 출력 불확실성 (Medium)

**문제**: LLM이 지시대로 `{"handoff_to": "..."}` JSON을 출력하지 않을 수 있음. LLM의 자유도로 인해 handoff 패턴이 불안정.

**관찰**:
- `llm-handoff` 테스트 (비스트리밍)는 성공
- `llm-streaming-handoff` 테스트는 실패
- 동일한 프롬프트도 LLM 응답에 따라 결과 다름

**제안 해결책**:
1. Structured output (JSON schema) 강제 사용
2. 더 명확한 프롬프트 + 예제 포함
3. Handoff 감지 정규식 완화

## 검증

```
# 빌드: 0 Error
dotnet build IronHive.slnx

# CI 테스트: 158 PASS
dotnet test tests/IronHive.Tests/IronHive.Tests.csproj

# 로컬 테스트: 50 total (47 PASS, 3 FAIL)
dotnet run --project tests/multi-agent-test/MultiAgentTest.csproj
```

## 코드 변경

### 신규 파일

없음 (테스트 코드만 추가)

### 수정 파일

| 파일 | 변경 내용 |
|------|----------|
| `tests/multi-agent-test/Program.cs` | LLM 통합 테스트 5개 + Environment Helpers 추가 |
| `tests/multi-agent-test/MultiAgentTest.csproj` | Provider 참조 (이미 있음) |

### 인터페이스 변경 (원격 반영)

| 파일 | 변경 내용 |
|------|----------|
| `IAgent.SystemPrompt` → `Instructions` | 원격에서 리네임, 로컬 코드 동기화 |

## 다음 사이클

- [ ] **P1 해결**: ISpeakerSelector에 초기 메시지 전달 기능 추가
- [ ] Handoff JSON 감지 개선 (정규식 완화 또는 structured output)
- [ ] 더 많은 LLM 테스트 시나리오 추가 (다양한 모델, 에지 케이스)
- [ ] Middleware 확장 (RetryMiddleware, LoggingMiddleware)
- [ ] Cycle 1/2 잔여 항목

## 프로젝트 철학 정렬

- **빌드타임 안전성**: 실제 LLM 호출로 런타임 검증 완료
- **커스텀 자유도 최대화**: LLM 기반 발언자 선택기 구조 검증 → 확장 필요성 확인
- **v0.x 과감한 설계**: ISpeakerSelector 인터페이스 확장 고려 중
