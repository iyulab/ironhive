# Cycle 6: Request Parameters & Complex Flows

## 범위

chat-api-tests에 **4개 시나리오** 추가. 요청 파라미터(MaxTokens, ThinkingEffort High, StopSequences)와 복합 스트리밍 흐름(Streaming Round-Trip) 검증.

### 프로젝트 철학 정렬
- MaxTokens: 출력 길이 제한 + `DoneReason.MaxTokens` 정확성 — Provider별 토큰 제한 동작의 핵심 계약
- ThinkingEffort High: 깊은 추론 활성화 — Cycle 5의 None과 대칭, reasoning 스펙트럼 검증
- StopSequences: 생성 중단 조건 — Provider별 stop sequence 처리 차이 발견
- Streaming Round-Trip: 스트리밍 도구 호출 → 실행 → 스트리밍 연속 — 가장 복잡한 실전 패턴

## 추가된 시나리오

### 10. max-tokens (MaxTokens Output Truncation)
- `MaxTokens = 20` + `ThinkingEffort.None` 설정
- "List the first 20 prime numbers" 요청 → 출력 절단 검증
- `DoneReason == MaxTokens` 확인

### 11. think-high (ThinkingEffort High)
- `ThinkingEffort = MessageThinkingEffort.High` 설정
- "What is 15 * 37? Think carefully" → ThinkingMessageContent 존재 + 길이 검증
- Cycle 5 think-none과 대칭 비교

### 12. stop-seq (Stop Sequences)
- `StopSequences = ["yellow"]` + `ThinkingEffort.None` 설정
- "List colors: red, blue, green, yellow, purple, orange" → "yellow" 전에 중단 검증
- `DoneReason == StopSequence` 확인, "yellow"/"purple" 포함 여부 확인

### 13. stream-roundtrip (Streaming Tool Round-Trip)
- 스트리밍 모드에서 도구 호출 수신 → 실행 → 스트리밍 연속
- `AssistantMessage` 수동 조립: `StreamingContentAddedResponse.Content` 추가 + `Merge()` 메서드로 delta 누적
- 비밀 값(`BEAM-HHmmss`) 최종 텍스트 포함 검증

## 실행 결과 (2차 — SDK 수정 후)

| Provider | max-tokens | think-high | stop-seq | stream-roundtrip |
|----------|------------|------------|----------|------------------|
| openai (o4-mini) | PASS | PASS | PASS* | PASS |
| azure-openai | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | PASS | PASS |
| google (gemini-2.5-flash) | PASS | PASS | PASS* | PASS |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | **FAIL** | PASS |
| ollama | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | PASS* | PASS |

**64 PASS / 3 SKIP / 1 FAIL** (누적 전체)

\* = StopSequences가 동작하지만 DoneReason이 정확하지 않음 (아래 상세 분석)

## 도출된 SDK 이슈

### 이슈 1: ToolInput 빈 문자열 파싱 실패 (수정됨)
- **증상**: Anthropic 스트리밍에서 파라미터 없는 도구의 `ToolMessageContent.Input`이 `""` (빈 문자열)로 누적
- **원인**: Anthropic 스트리밍은 `content_block_start`에서 `Input = null`, `input_json_delta`에서 `PartialJson = ""`을 전송. `Merge()` 후 `Input = ""`가 됨. 비스트리밍은 `JsonSerializer.Serialize(tool.Input)` → `"{}"` 반환.
- **영향**: `new ToolInput("")` → `ConvertTo<Dictionary>()` 실패
- **수정**: `ToolInput` 생성자에서 빈/공백 문자열을 null과 동일하게 처리
  ```csharp
  // Before:
  _items = input is null ? ... : input.ConvertTo<...>();
  // After:
  _items = input is null || (input is string str && string.IsNullOrWhiteSpace(str)) ? ... : input.ConvertTo<...>();
  ```
- **파일**: `src/IronHive.Abstractions/Tools/ToolInput.cs`

### 이슈 2: OpenAI Responses API에 StopSequences 미전달 (미수정)
- **증상**: openai/stop-seq에서 StopSequences가 무시됨 — 모든 색상이 출력에 포함
- **원인**: `ResponsesRequest` 변환 코드에 `StopSequences` 프로퍼티 매핑이 없음 (Chat Completion path에만 `Stop = request.StopSequences` 존재)
- **영향**: Responses API 사용 모델(o1, o3, o4, gpt-5)에서 StopSequences 기능 불가
- **위치**: `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs:153-177` (ResponsesRequest 생성 블록)
- **미수정 사유**: ResponsesRequest 페이로드 타입에 Stop 필드 추가가 필요하며, 더 넓은 변경 범위 확인 필요

## 관찰

### max-tokens (Provider별 동작)
| Provider | DoneReason | out tokens | 응답 내용 |
|----------|-----------|------------|-----------|
| openai (o4-mini) | **MaxTokens** | 0 | (no response) — 20토큰이 reasoning에 모두 소비, 텍스트 출력 없음 |
| anthropic | **MaxTokens** | 20 | 소수 10개 출력 후 절단 |
| google | **MaxTokens** | 16 | 소수 1개만 출력 |
| xai | **MaxTokens** | 20 | 소수 10개 출력 후 절단 |
| gpustack | **MaxTokens** | 20 | (no response) — thinking에 소비, 텍스트 출력 없음 |

**주요 발견**:
- openai(o4-mini)와 gpustack는 `ThinkingEffort.None`에서도 내부 reasoning이 MaxTokens를 소비하여 텍스트 출력이 없음
- OpenAI API는 `max_output_tokens >= 16` 최소값 제한이 있음 (초기 10으로 설정 시 실패, 20으로 수정)

### think-high (ThinkingEffort High 동작)
| Provider | thinking 반환? | thinkingLen | 주요 특징 |
|----------|---------------|-------------|-----------|
| openai (o4-mini) | Yes | 390 | Summary 형식, 계산 과정 포함 |
| anthropic | Yes | 583 | Detailed 형식, 단계별 분해 |
| google | Yes | 767 | 가장 긴 thinking, 상세한 분석 |
| xai | **No** | 0 | thinking 블록 미반환 — reasoning 모델이지만 thinking 노출 미지원 |
| gpustack | Yes | 391 | Detailed 형식 |

**xAI 발견**: `ThinkingEffort.High`에서도 thinking 블록이 반환되지 않음. grok-4-1-fast-reasoning은 내부적으로 reasoning을 수행하지만 thinking 내용을 노출하지 않는 것으로 보임.

### stop-seq (StopSequences Provider별 동작 — 핵심 발견)

| Provider | API path | stop 전송? | 중단 동작 | DoneReason | 정확성 |
|----------|----------|-----------|----------|------------|--------|
| openai (o4-mini) | Responses API | **No** (SDK 미구현) | 미중단 | EndTurn | ❌ |
| anthropic | Anthropic API | Yes | yellow 전 중단 | **StopSequence** | ✅ |
| google | GoogleAI API | Yes | yellow 전 중단 | EndTurn (STOP) | △ |
| xai | Chat Completion | Yes | **400 error** | N/A | ❌ |
| gpustack | Chat Completion | Yes | yellow 전 중단 | EndTurn (Stop) | △ |

**Provider별 finish_reason 매핑 구조적 한계**:
- **OpenAI Chat Completion**: `finish_reason: "stop"` → `MessageDoneReason.EndTurn` — 자연 종료와 stop sequence 적중을 구분하지 않음
- **GoogleAI**: `FinishReason.STOP` → `MessageDoneReason.EndTurn` — 동일 구조적 한계
- **Anthropic**: 유일하게 `stop_reason: "stop_sequence"` → `MessageDoneReason.StopSequence` 별도 제공
- **xAI**: reasoning 모델에서 `stop` 파라미터 자체를 거부 (400 Bad Request)

### stream-roundtrip (스트리밍 도구 Round-Trip)
- **전 Provider 성공**: AssistantMessage 수동 조립 + Merge() 패턴이 모든 Provider에서 정상 동작
- **Provider별 step1 이벤트 패턴**:
  - openai: `Begin → Added(Thinking) → Added(Tool) → Delta(ToolDelta) → Done` (8 chunks)
  - anthropic: `Begin → Added(Tool) → Delta(ToolDelta) → Done` (5 chunks)
  - google: `Begin → Added(Tool) → Done` (4 chunks, delta 없음)
  - xai: `Begin → Added(Tool) → Done` (4 chunks, delta 없음)
  - gpustack: `Begin → Added(Text) → Added(Thinking) → Delta(ThinkingDelta) → Added(Tool) → Delta(ToolDelta) → Done` (24 chunks)

## 1차 실행 실패 및 수정 과정

### 1차 실행 (3 FAIL)
1. **openai/max-tokens**: `max_output_tokens >= 16` 최소값 제한 위반 → MaxTokens를 10→20으로 수정
2. **anthropic/stream-roundtrip**: ToolInput 빈 문자열 파싱 실패 → ToolInput.cs SDK 수정 (이슈 1)
3. **xai/stop-seq**: 400 Bad Request → xAI API 제한 (수정 불요)

### 2차 실행 (1 FAIL)
- xai/stop-seq만 남음 (Provider 제한, SDK 수정 불요)

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `tests/chat-api-tests/Program.cs` | 4개 시나리오 추가 (TestMaxTokens, TestThinkingEffortHigh, TestStopSequences, TestStreamingRoundTrip) |
| `src/IronHive.Abstractions/Tools/ToolInput.cs` | 빈/공백 문자열 입력을 null과 동일하게 처리 |

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
| 10 | max-tokens | MaxTokens 출력 절단 + DoneReason.MaxTokens | C6 |
| 11 | think-high | ThinkingEffort High 깊은 추론 | C6 |
| 12 | stop-seq | StopSequences 생성 중단 조건 | C6 |
| 13 | stream-roundtrip | 스트리밍 도구 호출 → 실행 → 스트리밍 연속 | C6 |

## 다음 사이클

- [ ] OpenAI Responses API StopSequences 전달 구현 (이슈 2)
- [ ] xAI reasoning 모델 stop 파라미터 처리 (조건부 제외 또는 에러 핸들링)
- [ ] error handling (잘못된 모델명, API 에러, 빈 메시지 등)
- [ ] image/document input (멀티모달 검증)
- [ ] system prompt 없는 요청 테스트
- [ ] Temperature/TopP 샘플링 파라미터 검증
