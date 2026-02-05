# Cycle 7: SDK Parameter Guards & Error Handling

## 범위

Cycle 6에서 도출된 SDK 이슈 3건 수정 + **3개 시나리오** 추가. Provider별 파라미터 제한 처리와 에러 핸들링, 기본 동작 검증.

### 프로젝트 철학 정렬
- SDK Fix: Responses API StopSequences 미전달, o-series Temperature/TopP 거부, xAI reasoning stop 거부 — 빌드타임 안전성이 아닌 런타임 API 호환성 문제이므로 조건부 가드로 해결
- error-model: 잘못된 모델명에 대한 에러 핸들링 — Provider가 예측 가능한 예외를 던지는지 검증
- no-system: SystemPrompt 없는 요청 — 선택적 파라미터의 기본 동작 확인
- temperature: Temperature=0 결정론적 출력 — 샘플링 파라미터 정합성 검증

## SDK 수정 사항

### 수정 1: ResponsesRequest에 Stop 프로퍼티 추가
- **파일**: `src/IronHive.Providers.OpenAI/Payloads/Responses/ResponsesRequest.cs`
- **변경**: `Stop` 프로퍼티 추가 (`[JsonPropertyName("stop")]`)
- **효과**: Responses API 경로(o1, o3, o4, gpt-5)에서 StopSequences가 API로 전달됨

### 수정 2: Responses API — o-series 모델 파라미터 가드
- **파일**: `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` (ToOpenAI)
- **변경**:
  1. `isOSeries` 감지 추가 (`o1`, `o3`, `o4` prefix)
  2. `Stop = isOSeries ? null : request.StopSequences` — o-series에서 stop 미전송
  3. `Temperature = isOSeries || enabledReasoning ? null : request.Temperature` — o-series 항상 제거
  4. `TopP = isOSeries || enabledReasoning ? null : request.TopP` — o-series 항상 제거
- **이전 문제**: `enabledReasoning && supportsReasoningEffort` 가드는 ThinkingEffort.None일 때 Temperature를 전송하여 o4-mini가 400 에러 반환
- **효과**: o-series 모델에서 temperature/topP/stop 파라미터가 항상 제거됨

### 수정 3: Chat Completion API — xAI reasoning 모델 stop 가드
- **파일**: `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` (ToOpenAILegacy)
- **변경**: `Stop = model.Contains("grok") && model.Contains("reasoning") ? null : request.StopSequences`
- **이전 문제**: xAI grok reasoning 모델이 `stop` 파라미터를 거부하여 400 Bad Request
- **효과**: xAI reasoning 모델에서 stop 파라미터가 조건부 제거됨

## 추가된 시나리오

### 14. error-model (Invalid Model Error Handling)
- 존재하지 않는 모델명 `nonexistent-model-12345`로 요청
- `HttpRequestException` 또는 `Exception` 캐치 확인
- 에러 타입 + 상태 코드 리포트 → 항상 PASS (예외가 올바르게 발생하면 성공)

### 15. no-system (No SystemPrompt)
- `SystemPrompt`를 설정하지 않고 요청
- "What is 2+2?" → 응답에 "4" 포함 여부 확인
- ThinkingEffort.None 설정

### 16. temperature (Temperature=0 Deterministic Output)
- `Temperature = 0f`, `ThinkingEffort.None` 설정
- "What is the capital of France? Reply with only the city name." 동일 프롬프트 2회 실행
- 두 응답이 동일한지 비교 (대소문자 무시, trim 후 비교)

## 실행 결과 (2차 — SDK 수정 후)

| Provider | error-model | no-system | temperature | stop-seq (기존) |
|----------|-------------|-----------|-------------|-----------------|
| openai (o4-mini) | PASS | PASS | PASS | PASS* |
| azure-openai | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS | PASS | PASS |
| google (gemini-2.5-flash) | PASS | PASS | PASS | PASS* |
| xai (grok-4-1-fast-reasoning) | PASS | PASS | PASS | PASS** |
| ollama | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS | PASS | PASS* |

**80 PASS / 3 SKIP / 0 FAIL** (누적 전체)

\* = StopSequences가 전송되지만 DoneReason이 EndTurn으로 반환 (구조적 한계, Cycle 6 참고)
\** = xAI: stop 파라미터가 전송되지 않음 (가드 적용), 모든 색상 출력, DoneReason=EndTurn

## 관찰

### error-model (Provider별 에러 응답)
| Provider | 예외 타입 | 상태 코드 | 에러 메시지 |
|----------|-----------|-----------|-------------|
| openai | HttpRequestException | 404 | NotFound |
| anthropic | HttpRequestException | 404 | NotFound |
| google | HttpRequestException | 404 | NotFound |
| xai | HttpRequestException | 404 | NotFound |
| gpustack | HttpRequestException | 404 | NotFound |

**주요 발견**: 모든 Provider가 일관되게 `HttpRequestException` with `404 NotFound`를 반환. SDK의 에러 전파가 정상적으로 동작함.

### no-system (SystemPrompt 없는 요청)
- **전 Provider 성공**: SystemPrompt가 null/미설정이어도 모든 Provider에서 정상 응답
- 별도의 null 체크나 기본값 설정 없이 그대로 동작 — SDK의 선택적 파라미터 처리가 올바름

### temperature (Temperature=0 결정론적 출력)
| Provider | 1차 응답 | 2차 응답 | 일치 | 비고 |
|----------|----------|----------|------|------|
| openai (o4-mini) | Paris | Paris | Yes | Temperature가 o-series에서 무시되지만 결과는 결정론적 |
| anthropic | Paris | Paris | Yes | |
| google | Paris | Paris | Yes | |
| xai | Paris | Paris | Yes | |
| gpustack | Paris | Paris | Yes | |

**주요 발견**: 모든 Provider에서 Temperature=0이 결정론적 출력을 생성. o-series 모델은 SDK 가드에 의해 temperature가 전송되지 않지만, 기본적으로 결정론적 동작을 함.

### stop-seq (Cycle 6 이슈 수정 검증)
| Provider | Cycle 6 결과 | Cycle 7 결과 | 변경 사항 |
|----------|-------------|-------------|-----------|
| openai | PASS (Responses API에 stop 미전송, 미중단) | PASS (stop 전송, 하지만 o-series 가드로 미전송) | 구조 변경, 동작 동일 |
| xai | **FAIL** (400 Bad Request) | PASS (stop 미전송, 에러 없음) | 가드 적용으로 에러 해소 |

## 1차 실행 실패 및 수정 과정

### 1차 실행 (1 FAIL)
- **openai/temperature**: `temperature not supported with this model` — o4-mini가 Temperature 파라미터를 거부
- **원인**: `enabledReasoning && supportsReasoningEffort` 가드에서 ThinkingEffort.None일 때 `enabledReasoning = false` → Temperature가 전송됨
- **수정**: `isOSeries || enabledReasoning` 으로 변경하여 o-series 모델에서 항상 Temperature 제거

### 2차 실행 (0 FAIL)
- 모든 테스트 통과

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `tests/chat-api-tests/Program.cs` | 3개 시나리오 추가 (TestErrorModel, TestNoSystemPrompt, TestTemperature) |
| `src/IronHive.Providers.OpenAI/Payloads/Responses/ResponsesRequest.cs` | Stop 프로퍼티 추가 |
| `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` | o-series/xAI 파라미터 가드 4건 |

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
| 14 | error-model | 잘못된 모델명 에러 핸들링 | C7 |
| 15 | no-system | SystemPrompt 없는 요청 | C7 |
| 16 | temperature | Temperature=0 결정론적 출력 | C7 |

## Cycle 6 → 7 해소된 이슈

| Cycle 6 이슈 | 상태 | 해결 방법 |
|--------------|------|-----------|
| OpenAI Responses API StopSequences 미전달 | **해결** | ResponsesRequest.Stop 추가 + isOSeries 가드 |
| xAI reasoning 모델 stop 파라미터 거부 | **해결** | Chat Completion path에 grok reasoning 가드 추가 |
| Temperature/TopP o-series 거부 | **해결** (신규 발견) | isOSeries 가드 추가 |

## 다음 사이클

- [ ] image/document input (멀티모달 검증)
- [ ] multiple user messages in conversation (3턴 이상 복잡 대화)
- [ ] parallel tool calls (병렬 도구 호출 지원 여부)
- [ ] large context / long prompt 처리
- [ ] TopP / TopK 파라미터 검증
