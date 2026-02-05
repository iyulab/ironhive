# Cycle 4: Multi-Turn Conversation 검증

## 범위

chat-api-tests에 **multi-turn conversation 시나리오** 추가. 대화 히스토리(UserMessage + AssistantMessage)가 후속 턴에 정확히 전달되는지 검증.

### 프로젝트 철학 정렬
- Multi-turn은 chat의 기본 계약 — 모든 채팅 애플리케이션의 근간
- `IMessageGenerator`에 `Messages` 리스트로 이전 턴을 전달하는 패턴의 Provider별 직렬화 정확성 검증
- 테스트는 얇은 래퍼: 검증 가능한 secret 값으로 컨텍스트 유지만 확인

## 테스트 흐름

```
Turn 1: UserMessage("Remember this secret code: HIVE-xxxxxx") → AssistantMessage(확인 응답)
Turn 2: [UserMessage, AssistantMessage, UserMessage("What was the code?")] → TextMessageContent
검증: finalText.Contains(secretCode) → verified/not-found
```

## 실행 결과

| Provider | basic | streaming | tool-call | tool-roundtrip | multi-turn |
|----------|-------|-----------|-----------|----------------|------------|
| openai (o4-mini) | PASS | PASS | PASS | PASS | **PASS** |
| azure-openai | SKIP | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | PASS | PASS | **PASS** |
| google (gemini-2.5-flash) | PASS | PASS | PASS | PASS | **PASS** |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | PASS | PASS | **PASS** |
| ollama | SKIP | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | PASS | PASS | **PASS** |

**25 PASS / 3 SKIP / 0 FAIL**

## 도출된 SDK 이슈

**없음** — 모든 Provider에서 multi-turn 대화가 첫 실행에 성공. 이전 사이클들의 수정이 안정적으로 유지됨.

## 관찰

- **전 Provider context 검증 성공**: 모든 활성 Provider가 Turn 1의 secret code를 Turn 2에서 정확히 반환
- **AssistantMessage 직렬화**: Cycle 3(tool round-trip)에서 이미 검증한 AssistantMessage 재전송 패턴이 텍스트 전용(ThinkingMessageContent 포함) 경우에도 정상 동작
- **Provider별 응답 패턴 차이 없음**: 모든 Provider가 동일하게 secret code만 반환 (프롬프트 준수율 높음)

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `tests/chat-api-tests/Program.cs` | TestMultiTurn 시나리오 추가 |

## 누적 시나리오 현황

| # | 시나리오 | 검증 대상 | 도입 사이클 |
|---|----------|-----------|-------------|
| 1 | basic | 기본 생성, DoneReason, TokenUsage, Thinking | Cycle 1 |
| 2 | streaming | 스트리밍 이벤트 라이프사이클, 텍스트 수집 | Cycle 1 |
| 3 | tool-call | 도구 호출 감지, ToolMessageContent 반환 | Cycle 2 |
| 4 | tool-roundtrip | 도구 실행 → 결과 반환 → 최종 응답 | Cycle 3 |
| 5 | multi-turn | 대화 컨텍스트 유지 (2턴) | Cycle 4 |

## 다음 사이클

- [ ] streaming + tool calling 조합 테스트
- [ ] tool calling with parameters (인자 있는 도구)
- [ ] ThinkingEffort 전체 값 테스트 (None/Low/Medium/High)
- [ ] error handling (잘못된 모델명, 빈 메시지 등)
