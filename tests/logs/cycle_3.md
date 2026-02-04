# Cycle 3: Tool Calling Round-Trip 검증

## 범위

chat-api-tests에 **tool round-trip 시나리오** 추가. 도구 호출 → 실행 → 결과 반환 → 최종 텍스트 응답 전체 흐름을 `IMessageGenerator` 레벨에서 수동으로 검증.

### 프로젝트 철학 정렬
- Tool round-trip은 Agent 아키텍처의 핵심 계약 — `MessageService`의 자동 루프가 이 흐름 위에 구축됨
- `IMessageGenerator` 레벨 검증: Provider별 AssistantMessage(ToolMessageContent + Output) 직렬화 정확성 확인
- 테스트는 얇은 래퍼: SDK의 `FunctionTool.InvokeAsync` → `ToolOutput` → 재전송 패턴만 검증

## 테스트 구조 변경

- `RunScenarios`에 `TestToolRoundTrip` 시나리오 추가 (4번째)
- `using IronHive.Abstractions.Tools` 추가 (ToolInput, ToolOutput 사용)
- 검증 가능한 secret 값(`IRON-HHmmss`)을 반환하는 도구 정의
- 2단계 요청: (1) 도구 호출 수신 → 실행 → Output 설정, (2) AssistantMessage 재전송 → 최종 응답
- 최종 응답에 secret 값 포함 여부 검증

## 테스트 흐름

```
Step 1: UserMessage("Call get_secret_code") → Generator → ToolMessageContent(Input, Output=null)
Step 2: FunctionTool.InvokeAsync(ToolInput) → ToolOutput.Success("IRON-xxxxxx")
Step 3: ToolMessageContent.Output = toolOutput (직접 설정)
Step 4: [UserMessage, AssistantMessage(with Output)] → Generator → TextMessageContent("IRON-xxxxxx")
Step 5: finalText.Contains(secretValue) → verified/not-found
```

## 실행 결과

| Provider | basic | streaming | tool-call | tool-roundtrip |
|----------|-------|-----------|-----------|----------------|
| openai (o4-mini) | PASS | PASS | PASS | **PASS** |
| azure-openai | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | PASS | **PASS** |
| google (gemini-2.5-flash) | PASS | PASS | PASS | **PASS** |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | PASS | **PASS** |
| ollama | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | PASS | **PASS** |

**20 PASS / 3 SKIP / 0 FAIL**

## 도출된 SDK 이슈

**없음** — 모든 Provider에서 round-trip이 첫 실행에 성공. Cycle 2에서 수정한 tool schema 정규화가 이미 효과를 발휘.

## 관찰

- **전 Provider secret 검증 성공**: 모든 활성 Provider가 도구 실행 결과(IRON-xxxxxx)를 최종 응답에 정확히 포함
- **google**: 응답에 따옴표 포함 (`"IRON-xxxxxx"`) — Provider가 JSON 결과를 그대로 인용하는 패턴. `Contains()` 검증이므로 PASS
- **AssistantMessage 재전송**: 모든 Provider가 ToolMessageContent.Output이 포함된 AssistantMessage를 정상 처리. Provider별 직렬화(Responses API/Chat Completion/Anthropic/GoogleAI) 모두 정상

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `local-tests/chat-api-tests/Program.cs` | TestToolRoundTrip 시나리오 추가, using 추가 |

## 다음 사이클

- [ ] multi-turn conversation 시나리오 (대화 컨텍스트 유지 검증)
- [ ] streaming + tool calling 조합 테스트
- [ ] ThinkingEffort 전체 값 테스트 (None/Low/Medium/High)
- [ ] tool calling with parameters (인자 있는 도구)
