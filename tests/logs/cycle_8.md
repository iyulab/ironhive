# Cycle 8: Capability-Driven Request Building Refactoring + Regression

## 범위

OpenAI Provider의 모델명 문자열 검사(12건+)를 `ModelCapabilityResolver` 한 곳으로 집중하는 capability-driven 리팩토링. 기존 80 PASS / 3 SKIP / 0 FAIL 유지 확인.

### 프로젝트 철학 정렬
- **빌드타임 안전성**: 모델명 검사가 7개 파일에 분산 → 한 곳으로 집중. 새 모델 추가 시 변경 지점이 1곳으로 줄어 실수 가능성 감소
- **Registry+Builder 패턴 준수**: config 등록 시점에 벤더를 결정(`Compatibility`), 요청 빌드 시점에 capabilities를 resolve하는 2단계 구조
- **Provider 추상화**: `IMessageGenerator` 인터페이스 변경 없음 (breaking change 0건)

## 구현 내용

### 1. `OpenAICompatibility` enum + `OpenAIConfig.Compatibility` 프로퍼티
- **파일**: `src/IronHive.Providers.OpenAI/OpenAIConfig.cs`
- `Default(0)`, `XAI(1)`, `Azure(2)` — 벤더 식별
- 하위호환: 기존 코드는 `Default(0)`이 되므로 동작 변경 없음

### 2. `ModelCapabilityResolver.cs` (신규)
- **파일**: `src/IronHive.Providers.OpenAI/ModelCapabilityResolver.cs`
- `ModelCapabilities` record — 9개 boolean 플래그
- `ModelCapabilityResolver.Resolve(model, compatibility)` — 모든 모델명 검사가 여기에만 존재

| Compatibility | 모델 패턴 | UseResponsesApi | Instructions | SystemAsMsg | DeveloperMsg | Stop | Temp | TopP | ReasoningEffort | Reasoning |
|---------------|-----------|-----------------|-------------|-------------|-------------|------|------|------|-----------------|-----------|
| Default | o1/o3/o4 | true | true | false | true | **false** | **false** | **false** | true | true |
| Default | gpt-5+ | true | true | false | true | true | true | true | true | true |
| Default | -o1/-o3 포함 | false | true | false | false | true | true | true | true | true |
| Default | 기타 | false | true | false | false | true | true | true | true | false |
| XAI | grok-4* | false | **false** | **true** | false | **false** | **false** | **false** | **false** | false |
| XAI | 기타 grok | false | **false** | **true** | false | true | true | true | true | false |
| Azure | (Default 위임) | — | — | — | — | — | — | — | — | — |

### 3. `MessageGenerationRequestExtensions.cs` 리팩토링
- **파일**: `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs`
- `ToOpenAI(request)` → `ToOpenAI(request, caps)` — 모델명 검사 6건 제거
- `ToOpenAILegacy(request)` → `ToOpenAILegacy(request, caps)` — 모델명 검사 6건 제거
- 가시성: `public` → `internal` (어셈블리 내부에서만 호출)

### 4. Sub-generator에 `_compatibility` 저장
- `OpenAIChatMessageGenerator`: `_compatibility` 필드 추가, `Resolve()` 호출 후 `ToOpenAILegacy(caps)` 전달
- `OpenAIResponseMessageGenerator`: 동일 패턴

### 5. `OpenAIMessageGenerator.SelectGenerator()` 제거
- 모델명 기반 `SelectGenerator()` → `ModelCapabilityResolver.Resolve()` 기반 `caps.UseResponsesApi` 분기

### 6. 테스트 코드 업데이트
- xAI 등록에 `Compatibility = OpenAICompatibility.XAI` 추가
- Azure, LMStudio, GPUStack은 `Default`로 변경 불필요

## 회귀 테스트 결과

**80 PASS / 3 SKIP / 0 FAIL** — Cycle 7과 동일. 회귀 없음.

| Provider | 결과 | 모델 |
|----------|------|------|
| openai | 16/16 PASS | o4-mini |
| azure-openai | SKIP | (API 키 미설정) |
| anthropic | 16/16 PASS | claude-sonnet-4 |
| google | 16/16 PASS | gemini-2.5-flash |
| xai | 16/16 PASS | grok-4-1-fast-reasoning |
| ollama | SKIP | (미실행) |
| lmstudio | SKIP | (미실행) |
| gpustack | 16/16 PASS | gpt-oss-20b |

### Provider별 주요 확인 사항

- **openai (o4-mini)**: Responses API 라우팅 정상, stop/temp/topP 가드 유지
- **xai (grok-4-1-fast-reasoning)**: `Compatibility = XAI` 적용, system prompt가 input 메시지로 삽입, stop/temp/topP 가드 유지
- **gpustack (gpt-oss-20b)**: `Default` compatibility로 Chat Completion 정상 동작
- **anthropic/google**: OpenAI 프로바이더가 아니므로 이번 리팩토링 영향 없음, 기존 동작 유지 확인

## 검증

```
# 빌드: 0 Error / 42 Warning (모두 기존 경고)
dotnet build IronHive.slnx

# 모델명 검사가 Resolver에만 존재
grep StartsWith("grok  → ModelCapabilityResolver.cs만 매칭
grep StartsWith("o1    → ModelCapabilityResolver.cs만 매칭
grep Contains("-o1     → ModelCapabilityResolver.cs만 매칭

# 회귀 테스트: 80 PASS / 3 SKIP / 0 FAIL
tests/chat-api-tests/run.ps1
```

## 변경된 파일

| 파일 | 작업 |
|------|------|
| `src/IronHive.Providers.OpenAI/OpenAIConfig.cs` | `OpenAICompatibility` enum + `Compatibility` 프로퍼티 추가 |
| `src/IronHive.Providers.OpenAI/ModelCapabilityResolver.cs` | **NEW** — `ModelCapabilities` record + resolver |
| `src/IronHive.Providers.OpenAI/Extensions/MessageGenerationRequestExtensions.cs` | 양 메서드 시그니처 변경, 모델명 검사 12건 제거 |
| `src/IronHive.Providers.OpenAI/OpenAIMessageGenerator.cs` | `SelectGenerator` 제거, caps 기반 라우팅 |
| `src/IronHive.Providers.OpenAI/OpenAIChatMessageGenerator.cs` | `_compatibility` 저장, caps resolve + 전달 |
| `src/IronHive.Providers.OpenAI/OpenAIResponseMessageGenerator.cs` | `_compatibility` 저장, caps resolve + 전달 |
| `tests/chat-api-tests/Program.cs` | xAI 등록에 `Compatibility = XAI` 추가 |

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

## 향후 확장 시나리오

새 모델/벤더 추가 시 `ModelCapabilityResolver`에 케이스 하나만 추가:
```csharp
// 예: grok-5 출시 시
if (m.StartsWith("grok-5"))
    return xaiBase with { UseResponsesApi = true, SupportsReasoningEffort = true };
```

## 다음 사이클

- [ ] image/document input (멀티모달 검증)
- [ ] parallel tool calls (병렬 도구 호출 지원 여부)
- [ ] multiple user messages in conversation (3턴 이상 복잡 대화)
- [ ] large context / long prompt 처리
- [ ] TopP / TopK 파라미터 검증
