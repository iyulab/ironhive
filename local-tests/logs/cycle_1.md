# Cycle 1: Streaming 검증 추가

## 범위

chat-api-tests에 **streaming 시나리오** 추가. 기존 basic(non-streaming) 테스트만 존재하던 구조를 multi-scenario로 재설계.

### 프로젝트 철학 정렬
- Streaming은 `IMessageGenerator`의 핵심 계약 — 모든 Provider가 구현해야 하는 기본 기능
- 테스트는 얇은 래퍼로 유지: SDK의 streaming API 호출 + 라이프사이클 이벤트(Begin/Delta/Done) 검증만 수행

## 테스트 구조 변경

- `TestResult` 단건 반환 → `List<(string Scenario, TestResult)>` 다건 반환
- Provider별 `basic` + `streaming` 시나리오 실행
- `DoneReason`, `TokenUsage` 검증 추가 (basic/streaming 모두)
- Streaming: 모든 이벤트 타입 수집 (Begin/Added/Delta/Updated/Completed/Done/Error)
- ContentAdded + ContentDelta 양쪽에서 텍스트 수집 (Provider별 스트리밍 패턴 차이 대응)
- 결과 출력 포맷 개선 (provider별 그룹핑)

## 1차 실행 결과

| Provider | basic | streaming |
|----------|-------|-----------|
| openai (o4-mini) | PASS | **FAIL** |
| azure-openai | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS |
| google (gemini-2.5-flash) | PASS | **FAIL** |
| xai (grok-4-1-fast-reasoning) | PASS | PASS |
| ollama | SKIP | SKIP |
| lmstudio | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS |

**8 PASS / 3 SKIP / 2 FAIL**

## 도출된 SDK 이슈 및 수정

### 1. OpenAI Provider — streaming 시 'minimal' reasoning effort 전송

- **증상**: o4-mini 모델 streaming 요청 시 `"Unsupported value: 'minimal'"` 오류
- **원인**: `MessageGenerationRequestExtensions.cs:169` — ThinkingEffort switch의 default case가 `ResponsesReasoningEffort.Minimal`로 매핑
- **문제**: ThinkingEffort가 null일 때 enabledReasoning이 true → default case 진입 → `"minimal"` 직렬화 → o4-mini가 거부
- **수정**: default case를 `ResponsesReasoningEffort.Low`로 변경
- **검증**: openai/streaming PASS

### 2. GoogleAI Provider — streaming에서 텍스트 미수신

- **증상**: gemini-2.5-flash basic 정상, streaming에서 텍스트 없음
- **원인**: `GoogleAIGenerateContentClient.cs:40` — streaming 엔드포인트에 `?alt=sse` 쿼리 파라미터 누락
- **수정**: streaming 경로에 `?alt=sse` 추가
- **검증**: google/streaming PASS
- **추가 발견**: GoogleAI는 텍스트를 `StreamingContentAddedResponse` 단건으로 전송, `ContentDelta`는 0건. 테스트에서 ContentAdded 텍스트도 수집하도록 수정.

## 수정 후 최종 결과

| Provider | basic | streaming |
|----------|-------|-----------|
| openai (o4-mini) | PASS | **PASS** |
| azure-openai | SKIP | SKIP |
| anthropic (claude-sonnet-4) | PASS | PASS |
| google (gemini-2.5-flash) | PASS | **PASS** |
| xai (grok-4-1-fast-reasoning) | PASS | PASS |
| ollama | SKIP | SKIP |
| lmstudio | SKIP | SKIP |
| gpustack (gpt-oss-20b) | PASS | PASS |

**10 PASS / 3 SKIP / 0 FAIL**

## 기타 관찰

- **google/streaming**: 0 text deltas — 텍스트가 ContentAdded 하나에 담겨옴. thinking 모델 특성.
- **xai/streaming**: response `", 2, 3, 4, 5."` — "1" 누락. reasoning 모델이 thinking 과정에서 소비. SDK 이슈 아님.
- **gpustack/streaming**: DoneReason 빈 값. OpenAI-compatible 서버에서 finish_reason 미반환. 호환성 참고.

## 수정된 파일

| 파일 | 변경 |
|------|------|
| `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` | ThinkingEffort default → Low |
| `src/IronHive.Providers.GoogleAI/Clients/GoogleAIGenerateContentClient.cs` | streaming path에 `?alt=sse` 추가 |
| `local-tests/chat-api-tests/Program.cs` | multi-scenario 구조, streaming 시나리오, 이벤트 진단 |

## 다음 사이클

- [ ] tool calling 시나리오 추가 (Cycle 2)
- [ ] multi-turn conversation 시나리오
- [ ] ThinkingEffort 전체 값 테스트 (None/Low/Medium/High)
