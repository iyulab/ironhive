# Cycle 5: Advanced Chat Patterns

## 범위

chat-api-tests에 **4개 고급 시나리오** 일괄 추가. 넓은 범위로 SDK의 다양한 chat 패턴을 한 사이클에서 검증.

### 프로젝트 철학 정렬
- Tool with parameters: `FunctionTool`의 JSON Schema 파라미터 정의 + `BuildArguments` 인자 바인딩 — SDK의 도구 시스템 핵심
- Streaming tool call: 스트리밍에서 도구 호출 이벤트 라이프사이클 — Agent UX의 기반
- Multiple tools: `ToolCollection`에서 올바른 도구 선택 — 멀티도구 에이전트 패턴
- ThinkingEffort None: 추론 비활성화 옵션 — Provider별 reasoning 제어 정확성

## 추가된 시나리오

### 6. tool-params (Tool with Parameters)
- `add_numbers(a: int, b: int)` 도구 정의 (JSON Schema properties + required)
- "17 + 28" 계산 요청 → 도구 호출 수신 → `FunctionTool.InvokeAsync` → 결과 `45` 검증
- Input JSON 파싱 + 인자 바인딩까지 전체 흐름 검증

### 7. stream-tool (Streaming Tool Call)
- `check_status` 도구 정의 → 스트리밍 모드에서 호출 유도
- `StreamingContentAddedResponse`에서 `ToolMessageContent` 수신 검증
- 이벤트 라이프사이클: Begin → Added(Tool) → Delta(ToolDelta) → Done

### 8. multi-tool (Multiple Tools Selection)
- 3개 도구 정의: `get_weather(city)`, `get_time()`, `multiply(a,b)`
- "What's the weather in Seoul?" → `func_get_weather` 선택 검증
- Input에 `city: "Seoul"` 포함 검증

### 9. think-none (ThinkingEffort None)
- `ThinkingEffort = MessageThinkingEffort.None` 설정
- ThinkingMessageContent 반환 여부 확인
- Provider별 reasoning 비활성화 동작 차이 관찰

## 실행 결과

| Provider | tool-params | stream-tool | multi-tool | think-none |
|----------|-------------|-------------|------------|------------|
| openai (o4-mini) | PASS | PASS | PASS | PASS |
| azure-openai | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | PASS | PASS |
| google (gemini-2.5-flash) | PASS | PASS | PASS | PASS |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | PASS | PASS |
| ollama | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | PASS | PASS |

**45 PASS / 3 SKIP / 0 FAIL** (누적 전체)

## 도출된 SDK 이슈

**없음** — 4개 시나리오 모두 첫 실행에 전 Provider 통과.

## 관찰

### tool-params
- 전 Provider가 JSON Schema properties를 정확히 해석하고 올바른 인자 전달
- **google**: 파라미터 순서 반전 (`{"b":28,"a":17}`) — 비파괴적 차이, 이름 기반 바인딩이므로 정상
- **gpustack**: 키 사이에 공백 포함 (`{"a": 17, "b": 28}`) — 표준 JSON 포맷 차이, 정상

### stream-tool (Provider별 이벤트 패턴 차이)
- **openai**: `Begin → Added(Thinking) → Updated → Completed → Added(Tool) → Delta(ToolDelta) → Done` — Thinking 이벤트가 먼저 나옴
- **anthropic**: `Begin → Added(Text) → Delta(TextDelta) → Completed → Added(Tool) → Delta(ToolDelta) → Done` — 텍스트 먼저 후 도구
- **google**: `Begin → Added(Tool) → Completed → Done` — 가장 간결, Delta 없음
- **xai**: `Begin → Added(Tool) → Completed → Done` — google과 동일 패턴
- **gpustack**: `Begin → Added(Text) → Completed → Added(Thinking) → Delta(ThinkingDelta) → Added(Tool) → Delta(ToolDelta) → Done` — 가장 복잡, Text+Thinking+Tool 모두 포함

### multi-tool
- 전 Provider가 3개 도구 중 `func_get_weather`를 정확히 선택
- Input에 `city: "Seoul"` 정확히 포함
- 불필요한 추가 도구 호출 없음 (tools=1)

### think-none (ThinkingEffort None 동작 차이)
| Provider | thinking 반환? | 토큰 | 비고 |
|----------|---------------|------|------|
| openai (o4-mini) | **Yes** (1 block) | in=28, out=214 | None이어도 reasoning 발생. o-series 모델 특성 |
| anthropic | No (0 blocks) | in=27, out=6 | 정상 비활성화 |
| google | No (0 blocks) | in=19, out=23 | 정상 비활성화 |
| xai | No (0 blocks) | in=174, out=3 | 정상 비활성화 |
| gpustack | **Yes** (1 block) | in=91, out=34 | None이어도 thinking 발생. OpenAI-compatible 서버 특성 |

**주요 발견**: `ThinkingEffort.None`에서 openai(o4-mini)와 gpustack이 여전히 thinking 블록을 반환.
- **openai**: o-series는 `enabledReasoning = false`일 때 `Reasoning` 필드 미전송이지만, 모델 자체가 내부 reasoning을 수행하고 summary를 반환하는 것으로 보임. `Include: ["reasoning.encrypted_content"]` 미전송이므로 encrypted_content는 없지만 summary는 반환될 수 있음
- **gpustack**: OpenAI-compatible 서버에서 thinking 제어가 완전하지 않음. 호환성 참고

이는 SDK 결함이 아닌 Provider/모델별 동작 차이. 하지만 `ThinkingEffort.None`의 의미론적 보장이 Provider에 따라 다르다는 점은 문서화 가치가 있음.

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `local-tests/chat-api-tests/Program.cs` | 4개 시나리오 추가 (TestToolWithParams, TestStreamingToolCall, TestMultipleTools, TestThinkingEffortNone) |

## 누적 시나리오 현황

| # | 시나리오 | 검증 대상 | 도입 사이클 |
|---|----------|-----------|-------------|
| 1 | basic | 기본 생성, DoneReason, TokenUsage, Thinking | C1 |
| 2 | streaming | 스트리밍 이벤트 라이프사이클, 텍스트 수집 | C1 |
| 3 | tool-call | 도구 호출 감지, ToolMessageContent 반환 | C2 |
| 4 | tool-roundtrip | 도구 실행 → 결과 반환 → 최종 응답 | C3 |
| 5 | multi-turn | 대화 컨텍스트 유지 (2턴) | C4 |
| 6 | tool-params | JSON Schema 파라미터 + 인자 바인딩 | C5 |
| 7 | stream-tool | 스트리밍 도구 호출 이벤트 수신 | C5 |
| 8 | multi-tool | 다중 도구 중 올바른 선택 | C5 |
| 9 | think-none | ThinkingEffort None 추론 비활성화 | C5 |

## 다음 사이클

- [ ] ThinkingEffort.None에서 openai thinking 발생 조사 (Reasoning/Include 필드 제어 확인)
- [ ] error handling (잘못된 모델명, API 에러 등)
- [ ] image/document input (멀티모달 검증)
- [ ] max_tokens 제한 테스트
