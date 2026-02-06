# Fireworks AI Provider

**공식 문서**: https://docs.fireworks.ai/
**Base URL**: `https://api.fireworks.ai/inference/v1`
**API 타입**: Chat Completions API

---

## 호환성

- Chat Completions API
- Completions API (Legacy)
- Embeddings API
- Images API (FLUX, Stable Diffusion)
- Audio Transcription API (Whisper)
- Models API

---

## 모델 명명 규칙

Fireworks는 특수한 모델 ID 형식을 사용합니다:
```
accounts/fireworks/models/<model-id>
```
예: `accounts/fireworks/models/llama-v3p1-70b-instruct`

> 일부 모델은 짧은 이름도 지원합니다.

---

## 주요 모델 (카테고리별)

| 카테고리 | 모델 예시 |
|----------|-----------|
| Meta Llama | `llama-v3p1-70b-instruct`, `llama-v3p3-70b-instruct` |
| Qwen | `qwen2p5-72b-instruct`, Qwen Coder 시리즈 |
| DeepSeek | `deepseek-v3`, `deepseek-r1` |
| Mistral | `mixtral-8x22b-instruct` |
| Embedding | `nomic-ai/nomic-embed-text-v1.5`, `thenlper/gte-large` |

---

## 특수 파라미터

| 파라미터 | 타입 | 설명 |
|----------|------|------|
| `top_k` | `int` | Top-K 샘플링 |
| `min_p` | `float` | 최소 확률 샘플링 |
| `repetition_penalty` | `float` | 반복 패널티 |
| `prompt_truncate_len` | `int` | 프롬프트 최대 길이 초과 시 잘라냄 |
| `context_length_exceeded_behavior` | `string` | 컨텍스트 초과 시 동작: `"truncate"` 또는 `"error"` |

---

## Grammar 기반 구조화 출력 (Fireworks 전용)

```json
{
  "response_format": {
    "type": "grammar",
    "grammar": "<GBNF 문법 정의>"
  }
}
```

표준 `json_object`, `json_schema` 외에 GBNF 문법을 사용한 구조화 출력도 지원합니다.

---

## 지원 기능

- Function Calling / Tool Use
- JSON Mode / Structured Output
- Vision (select 모델)
- `logprobs` / `top_logprobs` 지원
- `n` > 1 지원

---

## 제한사항

- `logit_bias`: 미지원
- `web_search_options`: 미지원
- `reasoning_effort`: 미지원

---

## IronHive 구현

### Config 클래스: `FireworksConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |

### MessageGenerator: `FireworksMessageGenerator`

- `CompatibleChatMessageGenerator` 기반

### 사용 예시

```csharp
builder.AddFireworksProvider("fireworks", new FireworksConfig
{
    ApiKey = "your-api-key"
});
```

---

## TODO

- [ ] PostProcess에서 모델 ID prefix (`accounts/fireworks/models/`) 자동 처리
- [ ] `top_k`, `min_p`, `context_length_exceeded_behavior` 파라미터 지원
- [ ] Grammar 기반 구조화 출력 지원

---

*Last Updated: 2026-02-06*
