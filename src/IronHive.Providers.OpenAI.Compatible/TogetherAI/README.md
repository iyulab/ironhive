# Together AI Provider

**공식 문서**: https://docs.together.ai/docs/openai-api-compatibility
**Base URL**: `https://api.together.xyz/v1`
**API 타입**: Chat Completions API

---

## 호환성

- Chat Completions API
- Completions API (Legacy)
- Embeddings API
- Images API (FLUX, Stable Diffusion)
- Models API

---

## 주요 모델 (카테고리별)

| 카테고리 | 모델 예시 |
|----------|-----------|
| Meta Llama | `meta-llama/Llama-3.3-70B-Instruct`, Llama 3.2 Vision 시리즈 |
| Qwen | `Qwen/Qwen2.5-72B-Instruct`, Qwen Coder 시리즈 |
| DeepSeek | `deepseek-ai/DeepSeek-V3`, `deepseek-ai/DeepSeek-R1` |
| Mistral | `mistralai/Mixtral-8x22B-Instruct-v0.1` |
| Google | `google/gemma-2-27b-it` |
| Embedding | `BAAI/bge-large-en-v1.5`, `togethercomputer/m2-bert-80M-*` |

---

## 특수 파라미터

| 파라미터 | 타입 | 설명 |
|----------|------|------|
| `top_k` | `int` | Top-K 샘플링 (0-100) |
| `repetition_penalty` | `float` | 반복 패널티 (0.0-2.0) |
| `min_p` | `float` | 최소 확률 샘플링 (0.0-1.0) |
| `safety_model` | `string` | 콘텐츠 모더레이션 모델 (예: `Meta-Llama/LlamaGuard-2-8b`) |

---

## 지원 기능

- Function Calling / Tool Use (select 모델)
- JSON Mode (`response_format: {"type": "json_object"}`)
- Structured Output (`response_format: {"type": "json_schema", ...}`)
- Vision (Llama 3.2 Vision 모델)
- `logprobs` (정수형, 0-20)
- `n` > 1 지원

---

## 제한사항

- `logit_bias`: 미지원
- `web_search_options`: 미지원 (내장 검색 없음)
- `reasoning_effort`: 미지원
- Responses API: 미지원

---

## IronHive 구현

### Config 클래스: `TogetherAIConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |

### MessageGenerator: `TogetherAIMessageGenerator`

- `CompatibleChatMessageGenerator` 기반

### 사용 예시

```csharp
builder.AddTogetherAIProvider("together", new TogetherAIConfig
{
    ApiKey = "your-api-key"
});
```

---

## TODO

- [ ] PostProcess에서 `top_k`, `repetition_penalty`, `min_p` 파라미터 지원
- [ ] `safety_model` 파라미터 지원

---

*Last Updated: 2026-02-06*
