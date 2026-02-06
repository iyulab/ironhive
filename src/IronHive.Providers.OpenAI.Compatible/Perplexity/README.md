# Perplexity Provider

**공식 문서**: https://docs.perplexity.ai/
**Base URL**: `https://api.perplexity.ai`
**API 타입**: Chat Completions API **만 지원**

> **주의**: Perplexity는 Responses API, Embeddings API, Models API를 지원하지 않습니다.
> URL에 `/v1` 프리픽스가 없습니다. 엔드포인트: `https://api.perplexity.ai/chat/completions`

---

## 호환성

- Chat Completions API **만 지원**

---

## 모델

| 모델 | 설명 | 검색 | 추론 |
|------|------|------|------|
| `sonar-deep-research` | 다단계 심층 연구 에이전트 | O | O |
| `sonar-reasoning-pro` | 추론 + 검색 (확장 thinking) | O | O |
| `sonar-reasoning` | 추론 + 검색 | O | O |
| `sonar-pro` | 고급 검색 모델 (더 정밀) | O | X |
| `sonar` | 표준 검색 모델 (빠름) | O | X |
| `r1-1776` | DeepSeek R1 (오프라인, 무검열) | X | O |

> **sonar 계열**: 모든 쿼리에서 자동으로 웹 검색을 수행합니다.
> **r1-1776**: 검색 없는 순수 추론 모델. `citations`/`search_results` 없음. `reasoning_content` 포함.

---

## 특수 요청 파라미터

| 파라미터 | 타입 | 설명 |
|----------|------|------|
| `search_domain_filter` | `string[]` | 검색 도메인 제한 (예: `["wikipedia.org"]`, 제외: `["-reddit.com"]`) |
| `return_images` | `bool` | 이미지 결과 반환 (기본: `false`) |
| `return_related_questions` | `bool` | 관련 후속 질문 반환 (기본: `false`) |
| `search_recency_filter` | `string` | 검색 시간 필터: `"month"`, `"week"`, `"day"`, `"hour"` |
| `top_k` | `int` | Top-K 샘플링 (0-2048) |

---

## 응답 특수 필드 (매우 중요)

Perplexity 응답은 표준 OpenAI 형식에 추가 필드를 포함합니다:

```json
{
  "citations": [
    "https://example.com/source1",
    "https://example.com/source2"
  ],
  "search_results": [
    {
      "title": "출처 제목",
      "url": "https://example.com/source1",
      "snippet": "관련 발췌..."
    }
  ],
  "images": [
    {
      "image_url": "https://...",
      "origin_url": "https://...",
      "height": 600,
      "width": 800
    }
  ],
  "related_questions": [
    "후속 질문 1?",
    "후속 질문 2?"
  ],
  "choices": [{
    "message": {
      "role": "assistant",
      "content": "답변 텍스트에 [1] 인라인 인용 참조..."
    }
  }]
}
```

- **인용 참조**: 텍스트 내 `[1]`, `[2]` 등은 `citations` 배열의 1-indexed 참조입니다.
- **스트리밍**: `citations` 배열은 마지막 청크 (`finish_reason` 포함)에서만 전달됩니다.
- **r1-1776**: `reasoning_content` 필드가 `message`에 포함됩니다.

---

## 제한사항

| 파라미터 | 제한 |
|----------|------|
| `n` | 1만 지원 |
| `tools` / `tool_choice` | **미지원** (Function Calling 없음) |
| `response_format` | **미지원** (JSON Mode 없음) |
| `logit_bias` / `logprobs` / `top_logprobs` | 미지원 |
| `seed` | 미지원 |
| System 메시지 | **1개만 허용**, 반드시 첫 번째 메시지 |
| Responses API | 미지원 |
| Embeddings API | 미지원 |

---

## IronHive 구현

### Config 클래스: `PerplexityConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |

### MessageGenerator: `PerplexityMessageGenerator`

- `CompatibleChatMessageGenerator` 기반

### 사용 예시

```csharp
builder.AddPerplexityProvider("perplexity", new PerplexityConfig
{
    ApiKey = "your-api-key"
});
```

---

## TODO

- [ ] PostProcess에서 검색 파라미터 주입 (`search_domain_filter`, `return_images` 등)
- [ ] PostProcess에서 미지원 파라미터 제거 (`tools`, `response_format` 등)
- [ ] PostProcessResponse에서 `citations`, `search_results`, `images`, `related_questions` 추출
- [ ] System 메시지 1개 제한 처리

---

*Last Updated: 2026-02-06*
