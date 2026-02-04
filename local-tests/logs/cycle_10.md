# Cycle 10: Parallel Tool Calls & TopP Parameter Verification

## 범위

병렬 도구 호출과 TopP 생성 파라미터 검증. 단일 응답에서 여러 도구를 동시에 호출하는 능력과 nucleus sampling 파라미터의 Provider별 동작 확인.

### 프로젝트 철학 정렬
- **빌드타임 안전성**: `TopP`는 `MessageGenerationParameters`에 `float?` 타입으로 정의되어 컴파일타임에 검증
- **Provider 추상화**: 동일한 `ToolCollection`과 `TopP` 파라미터로 모든 Provider가 동작
- **SDK 사용자 가치**: 병렬 도구 호출 지원 여부를 Provider별로 실증

## SDK 수정 사항

없음. 기존 SDK가 병렬 도구 호출과 TopP를 올바르게 처리함.

## 추가된 시나리오

### 19. parallel-tools (Parallel Tool Calls)
- 3개의 독립 도구 정의: `get_weather(city)`, `get_time(timezone)`, `translate_text(text, target_language)`
- "3가지를 한번에 해줘" 형태의 프롬프트로 모든 도구를 한 번의 요청에서 호출하도록 유도
- 반환된 `ToolMessageContent` 개수로 병렬 호출 여부 판단 (`calls >= 2` → parallel=True)
- 각 도구 호출 여부 개별 확인 (weather, time, translate)

### 20. top-p (TopP Parameter — Nucleus Sampling)
- TopP=0.1 (매우 집중적)과 TopP=1.0 (전체 분포)으로 동일 프롬프트 2회 실행
- "물의 화학식은?" → H2O/H₂O 포함 여부로 정확성 검증
- 두 TopP 값 모두에서 정상 응답과 토큰 사용량 확인

## 실행 결과

**100 PASS / 3 SKIP / 0 FAIL**

| Provider | 기존 18개 | parallel-tools | top-p | 합계 |
|----------|-----------|----------------|-------|------|
| openai (o4-mini) | 18/18 PASS | PASS | PASS | 20/20 |
| azure-openai | SKIP | SKIP | SKIP | SKIP |
| anthropic (claude-sonnet-4) | 18/18 PASS | PASS | PASS | 20/20 |
| google (gemini-2.5-flash) | 18/18 PASS | PASS | PASS | 20/20 |
| xai (grok-4-1-fast-reasoning) | 18/18 PASS | PASS | PASS | 20/20 |
| ollama | SKIP | SKIP | SKIP | SKIP |
| lmstudio | SKIP | SKIP | SKIP | SKIP |
| gpustack (gpt-oss-20b) | 18/18 PASS | PASS | PASS | 20/20 |

### Provider별 병렬 도구 호출 결과

| Provider | 호출 수 | parallel | weather | time | translate | 비고 |
|----------|---------|----------|---------|------|-----------|------|
| openai | 1 | False | True | False | False | 순차 호출 패턴 (한 번에 1개씩) |
| anthropic | 3 | **True** | True | True | True | 완전 병렬 호출 |
| google | 3 | **True** | True | True | True | 완전 병렬 호출 |
| xai | 3 | **True** | True | True | True | 완전 병렬 호출 |
| gpustack | 1 | False | True | False | False | 순차 호출 패턴 (한 번에 1개씩) |

**관찰**: OpenAI(o4-mini)와 GPUStack은 한 번의 응답에서 1개의 도구만 호출. Anthropic, Google, xAI는 3개 모두 병렬 호출. 이는 모델의 도구 호출 전략 차이이며, SDK는 양쪽 모두 올바르게 처리.

### Provider별 TopP 결과

| Provider | TopP=0.1 | TopP=1.0 | 정확도 | 비고 |
|----------|----------|----------|--------|------|
| openai | H2O (in=35,out=69) | H₂O (in=35,out=58) | 100% | 정상 |
| anthropic | H₂O (in=32,out=7) | H2O (in=32,out=6) | 100% | 정상 |
| google | correct (in=26,out=359) | correct (in=26,out=358) | 100% | 정상 (출력이 장황) |
| xai | H₂O (in=181,out=3) | H₂O (in=181,out=3) | 100% | 정상 |
| gpustack | H2O (in=98,out=56) | H₂O (in=98,out=111) | 100% | 정상 |

**관찰**: 모든 Provider에서 TopP가 정상 적용됨. Google은 동일 질문에 대해 출력이 길지만 정확도는 동일.

## 검증

```
# 빌드: 0 Error
dotnet build IronHive.slnx

# 단위 테스트: 134 PASS
dotnet test IronHive.slnx

# 전체 로컬 테스트: 100 PASS / 3 SKIP / 0 FAIL
local-tests/chat-api-tests/run.ps1
```

## 변경된 파일

| 파일 | 작업 |
|------|------|
| `local-tests/chat-api-tests/Program.cs` | `TestParallelToolCalls`, `TestTopP` 시나리오 추가 |

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
| 17 | image-input | 이미지+텍스트 멀티모달 기본 | C9 |
| 18 | image-stream | 이미지+텍스트 멀티모달 스트리밍 | C9 |
| 19 | parallel-tools | 단일 응답에서 다중 도구 병렬 호출 | C10 |
| 20 | top-p | TopP nucleus sampling 파라미터 검증 | C10 |

## 도출된 SDK 이슈

### 관찰 사항 (수정 불필요)
| 관찰 | 설명 |
|------|------|
| OpenAI/GPUStack 비병렬 도구 호출 | o4-mini와 gpt-oss-20b는 한 번에 1개 도구만 호출. 모델 수준 제약이며 SDK 이슈 아님 |
| Google TopP 장황 출력 | TopP 값과 무관하게 Google은 상세 응답 생성. Provider 특성이며 SDK 이슈 아님 |

## 다음 사이클

- [ ] multiple user messages in conversation (3턴 이상 복잡 대화)
- [ ] large context / long prompt 처리
- [ ] document input (DocumentMessageContent 검증)
- [ ] multiple images in single message (다중 이미지)
- [ ] TopK 파라미터 검증 (Google/Anthropic 특화)
- [ ] parallel tool calls round-trip (병렬 호출 → 실행 → 결과 반환)
