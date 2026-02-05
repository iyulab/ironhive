# IronHive OpenAI Providers 리팩토링 TODO

## 현재 진행 상황 요약

### ✅ 완료된 작업

1. **기본 아키텍처 설계**
   - `CompatibleConfig` 추상 클래스 (ToOpenAI() 메서드 패턴)
   - `CompatibleChatMessageGenerator` (Chat Completions API 기반)
   - `CompatibleResponseMessageGenerator` (Responses API 기반)

2. **Provider 폴더 구조 생성**
   - XAI, Groq, DeepSeek, TogetherAI, Fireworks, Perplexity, OpenRouter
   - 각 Provider별 Config, MessageGenerator 클래스

3. **Extension 메서드**
   - `AddxAIProvider`, `AddGroqProvider`, `AddDeepSeekProvider` 등 7개 메서드
   - `AddCompatibleProvider` (self-hosted용 범용)

4. **문서화**
   - README.md에 각 Provider별 특이사항 정리

---

## 🔧 핵심 수정 필요 사항

### 1. PostProcess 메서드 연결 (Critical)

**문제점**: `OpenAIChatMessageGenerator`와 `OpenAIResponseMessageGenerator`에 PostProcess 메서드들이 정의되어 있지만, 실제 로직에서 호출되고 있지 않음.

**파일 위치**:
- `OpenAIChatMessageGenerator.cs` (Lines 341-366)
- `OpenAIResponseMessageGenerator.cs` (Lines 311-339)

**수정 필요**:
```csharp
// GenerateMessageAsync에서 request/response 처리 전후에 호출 필요
var req = PostProcessRequest<ChatCompletionRequest>(request.ToOpenAILegacy());
var res = PostProcessResponse<ChatCompletionResponse>(await _client.PostChatCompletionAsync(req));

// GenerateStreamingMessageAsync에서도 동일하게 적용
```

**영향 범위**: 모든 Provider에서 요청/응답 커스터마이징 가능해짐

---

### 2. Provider별 특수 파라미터 Request 반영

현재 Config에 정의된 특수 설정들이 실제 API 요청에 반영되지 않음.

#### 2.1 XAI (Grok)

**Config 설정 (정의됨)**:
- `EnableSearch`, `SearchParameters`, `Store`, `PreviousResponseId`

**필요 작업**:
- [ ] `XAIMessageGenerator`에서 `PostProcessRequest` 오버라이드
- [ ] ResponsesRequest에 xAI 전용 파라미터 추가
  ```json
  {
    "search_enabled": true,
    "search_parameters": { "max_search_results": 10 },
    "store": false,
    "previous_response_id": "resp_xxx"
  }
  ```
- [ ] Server-side tools 지원: `web_search`, `x_search`, `code_execution`

#### 2.2 DeepSeek

**현재 누락된 Config 설정**:
- `ThinkingMode` (enabled/disabled)
- `PrefixCompletion` (beta endpoint 사용 여부)
- `BetaBaseUrl` (https://api.deepseek.com/beta)

**필요 작업**:
- [ ] `DeepSeekConfig`에 Thinking 설정 추가
- [ ] `DeepSeekMessageGenerator`에서 `PostProcessRequest` 오버라이드
- [ ] Request에 thinking 파라미터 추가
  ```json
  { "thinking": { "type": "enabled" } }
  ```
- [ ] Prefix completion assistant 메시지 지원
- [ ] 응답의 `reasoning_content` 필드 처리 (`PostProcessResponse`)

#### 2.3 OpenRouter

**Config 설정 (정의됨, 미반영)**:
- `Transforms`, `Route`, `ProviderPreferences`

**필요 작업**:
- [ ] `OpenRouterMessageGenerator`에서 `PostProcessRequest` 오버라이드
- [ ] Request body에 특수 파라미터 추가
  ```json
  {
    "transforms": ["middle-out"],
    "route": "fallback",
    "provider": {
      "allow_fallbacks": true,
      "require_parameters": false,
      "order": ["openai", "anthropic"]
    }
  }
  ```
- [ ] 응답의 `native_finish_reason` 처리 (`PostProcessResponse`)

#### 2.4 Groq

**필요 작업**:
- [ ] `GroqConfig`에 제한사항 검증 옵션 추가
- [ ] `PostProcessRequest`에서 `n` 파라미터 강제 1로 설정
- [ ] `presence_penalty`, `logit_bias`, `logprobs` 등 미지원 파라미터 제거

#### 2.5 Perplexity

**필요 작업**:
- [ ] `PerplexityConfig`에 검색 관련 설정 추가
- [ ] 응답의 citations 처리

---

### 3. Payload 확장 메커니즘

**문제점**: Provider별 특수 파라미터를 Request에 추가할 표준화된 방법 없음

**제안 방식**:

#### 방식 A: JsonExtensionData 활용
```csharp
public class ChatCompletionRequest
{
    // 기존 속성들...

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
```

#### 방식 B: Provider별 Request 상속
```csharp
public class DeepSeekChatCompletionRequest : ChatCompletionRequest
{
    [JsonPropertyName("thinking")]
    public DeepSeekThinking? Thinking { get; set; }
}
```

#### 방식 C: Client에서 JsonObject 병합
```csharp
protected virtual void ModifyRequestJson(JsonObject requestJson) { }
```

**권장**: 방식 A + C 조합 (유연성과 타입 안정성 균형)

---

### 4. Response 확장 처리

**필요 작업**:
- [ ] `ChatCompletionResponse`, `StreamingChatCompletionResponse`에 `Raw` 속성 추가 (JsonObject)
- [ ] `JsonPayloadResponse` 기반 클래스 활용
- [ ] Provider별 `PostProcessResponse`에서 Raw JSON 파싱하여 특수 필드 추출

---

## 📋 추가 구현 작업

### 5. Embedding 지원

**지원 Provider**:
- OpenRouter (Embeddings API)
- Fireworks (Embeddings API)
- Together AI (Embeddings API)

**필요 작업**:
- [ ] `CompatibleEmbeddingGenerator` 클래스 생성
- [ ] Provider별 Embedding Config 설정
- [ ] Extension 메서드에 Embedding 등록 옵션 추가

### 6. Model Catalog

**필요 작업**:
- [ ] `CompatibleModelCatalog` 클래스 생성
- [ ] Provider별 모델 목록 조회 API 연동
- [ ] OpenRouter의 다중 모델 목록 처리

### 7. 에러 처리

**필요 작업**:
- [ ] Provider별 에러 응답 포맷 처리
- [ ] GPUStack의 스트리밍 에러 (`error:` prefix) 처리
- [ ] Rate limiting 에러 처리 및 재시도 로직

---

## 🏗️ 리팩토링 제안

### 8. MessageGenerationRequest 확장

현재 `ToOpenAI()`와 `ToOpenAILegacy()` 확장 메서드가 Provider 특수 기능을 지원하지 않음.

**제안**:
```csharp
// Provider 컨텍스트를 받는 오버로드 추가
public static ChatCompletionRequest ToOpenAILegacy(
    this MessageGenerationRequest request,
    IProviderContext? context = null)
{
    var req = /* 기존 변환 로직 */;
    context?.ApplyToRequest(req);
    return req;
}
```

### 9. Config → Client 의존성 주입

현재 구조:
```
Config → ToOpenAI() → OpenAIConfig → MessageGenerator → Client
```

제안 구조 (Provider 특수 설정 전달 가능):
```
Config → MessageGenerator(Config) → Client(OpenAIConfig) + RequestModifier(Config)
```

---

## 📌 우선순위 정리

### Phase 1 (핵심 기능)
1. [ ] PostProcess 메서드 연결 (OpenAIChatMessageGenerator, OpenAIResponseMessageGenerator)
2. [ ] XAI 특수 파라미터 구현 (search, store)
3. [ ] DeepSeek Thinking mode 구현

### Phase 2 (Provider 완성)
4. [ ] OpenRouter 특수 파라미터 구현
5. [ ] Groq 제한사항 처리
6. [ ] Perplexity citations 처리

### Phase 3 (추가 기능)
7. [ ] Embedding 지원
8. [ ] Model Catalog
9. [ ] 에러 처리 강화

---

## 📝 참고 사항

### 테스트 필요 항목
- 각 Provider별 실제 API 호출 테스트
- Streaming 응답 처리 검증
- Tool calling 동작 확인

### 주의 사항
- OpenAI의 Chat Completions와 Responses API 규격 차이
- Provider별 지원/미지원 파라미터 차이
- Rate limiting 및 quota 관리

---

*Last Updated: 2026-02-05*
