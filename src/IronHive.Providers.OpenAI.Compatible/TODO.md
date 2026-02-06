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

### 0. Provider별 문서화 작업
- README.md파일에 각 Provider별 특이사항 및 설정 방법 수정필요
- 아직 실제 반영되지 않은 설정들에 대한 설명 추가 필요(웹조사로 다시 확인)

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

**필요 작업**:
- 각 Provider별 PostProcessRequest에서 특수 파라미터를 Request에 추가

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

*Last Updated: 2026-02-05*
