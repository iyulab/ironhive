# Cycle 2: Tool Calling 검증

## 범위

chat-api-tests에 **tool calling 시나리오** 추가. 모든 Provider에서 FunctionTool 정의 → 요청 → ToolCall 응답 수신 흐름 검증.

### 프로젝트 철학 정렬
- Tool calling은 `IMessageGenerator`의 핵심 계약 — Agent 아키텍처의 기반 기능
- 테스트는 얇은 래퍼: FunctionTool 생성 → MessageGenerationRequest에 Tools 전달 → ToolMessageContent 반환 검증
- 인자 없는(no-parameter) 도구로 최소 검증. SDK의 도구 직렬화 정확성에 집중

## 테스트 구조 변경

- `RunScenarios`에 `TestToolCalling` 시나리오 추가 (basic, streaming에 이어 3번째)
- `FunctionTool(Delegate)` 패턴으로 `get_current_time` 도구 생성
- `ToolCollection` 래퍼 사용 (SDK 표준 패턴)
- 응답에서 `ToolMessageContent` 추출, 도구 호출 수/이름 검증
- `DoneReason == ToolCall` 검증

## 1차 실행 결과

| Provider | basic | streaming | tool-call |
|----------|-------|-----------|-----------|
| openai (o4-mini) | PASS | PASS | **FAIL** |
| azure-openai | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | **FAIL** |
| google (gemini-2.5-flash) | PASS | PASS | PASS |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | **FAIL** |
| ollama | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | PASS |

**13 PASS / 3 SKIP / 2 FAIL** (1차), 이후 anthropic 수정 후 **14 PASS / 3 SKIP / 1 FAIL** (2차)

## 도출된 SDK 이슈 및 수정

### 공통 원인: Parameters null일 때 빈 JSON Schema 전송

모든 Provider에서 `FunctionTool.Parameters`가 null일 때 `new JsonObject()` (빈 `{}`)를 전송하고 있었음. 이는 유효한 JSON Schema가 아님.

### 1. Anthropic Provider — `input_schema.type` 필드 누락

- **증상**: `tools.0.custom.input_schema.type: Field required`
- **원인**: `Anthropic/Extensions/MessageGenerationRequestExtensions.cs:169` — `InputSchema = t.Parameters ?? new JsonObject()` → 빈 `{}`에 `type` 필드 없음
- **수정**: `new JsonObject { ["type"] = "object" }`
- **검증**: anthropic/tool-call PASS

### 2. OpenAI Provider (Responses API) — `object schema missing properties`

- **증상**: `Invalid schema for function 'func_get_current_time': In context=(), object schema missing properties.`
- **원인**: `OpenAI/Extensions/MessageGenerationRequestExtensions.cs:164` — Responses API는 `type` + `properties` 둘 다 요구
- **수정**: `new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }`
- **검증**: openai/tool-call PASS

### 3. OpenAI Provider (Chat Completion API, xAI 경로) — 422 Unprocessable Entity

- **증상**: xAI `grok-4-1-fast-reasoning` 모델에서 422 오류
- **원인**: `OpenAI/Extensions/MessageGenerationRequestExtensions.cs:313` — ChatCompletion 경로에서 `Parameters = t.Parameters` (null) → `WhenWritingNull` 옵션으로 `parameters` 필드 자체가 누락 → xAI가 거부
- **배경**: xAI는 `SelectGenerator`에서 grok-* 모델이 o-series가 아니므로 ChatCompletion API 사용. gpustack은 같은 경로인데 null parameters 허용 (더 관대함)
- **수정**: `new JsonObject { ["type"] = "object", ["properties"] = new JsonObject() }`
- **검증**: xai/tool-call PASS

### 4. GoogleAI Provider — 사전 수정 (동일 패턴)

- GoogleAI도 `new JsonObject()`를 사용하고 있었으나, Google API가 빈 `{}`를 허용해서 테스트 통과
- 일관성을 위해 `new JsonObject { ["type"] = "object" }`로 함께 수정

## 수정 후 최종 결과

| Provider | basic | streaming | tool-call |
|----------|-------|-----------|-----------|
| openai (o4-mini) | PASS | PASS | **PASS** |
| azure-openai | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | **PASS** |
| google (gemini-2.5-flash) | PASS | PASS | **PASS** |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | **PASS** |
| ollama | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | **PASS** |

**15 PASS / 3 SKIP / 0 FAIL**

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` | Responses API: `{"type":"object","properties":{}}`, ChatCompletion API: 동일 |
| `src/IronHive.Providers.Anthropic/Extensions/MessageGenerationRequestExtensions.cs` | `{"type":"object"}` |
| `src/IronHive.Providers.GoogleAI/Extensions/MessageGenerationRequestExtensions.cs` | `{"type":"object"}` |
| `tests/chat-api-tests/Program.cs` | TestToolCalling 시나리오 추가 |

## 설계 인사이트

- **JSON Schema 호환성**: 인자 없는 도구라도 유효한 JSON Schema를 보내야 함. `{"type": "object"}` 최소, OpenAI는 `properties`도 요구
- **Provider별 관대함**: Google/GPUStack은 빈 `{}`도 허용, Anthropic/OpenAI/xAI는 엄격
- **API 경로 분기**: xAI는 grok-* 모델이므로 ChatCompletion 경로 사용. 같은 OpenAI Provider 코드 내에서도 Responses/ChatCompletion 양쪽 모두 수정 필요
- **FunctionTool.UniqueName**: SDK가 자동으로 `func_` 접두사 추가 (`get_current_time` → `func_get_current_time`)

## 다음 사이클

- [ ] tool calling + execution 라운드트립 (도구 실행 후 후속 메시지 생성)
- [ ] multi-turn conversation 시나리오
- [ ] ThinkingEffort 전체 값 테스트 (None/Low/Medium/High)
- [ ] streaming + tool calling 조합 테스트
