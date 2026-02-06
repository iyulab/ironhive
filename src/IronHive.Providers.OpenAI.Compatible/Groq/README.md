# Groq Provider

**공식 문서**: https://console.groq.com/docs/overview
**API Reference**: https://console.groq.com/docs/api-reference
**Base URL**: `https://api.groq.com/openai/v1`
**API 타입**: Chat Completions API

---

## 호환성

- Chat Completions API (주요 엔드포인트)
- Responses API (Beta)
- Models API
- Audio Transcription/Translation API (Whisper 모델)

---

## 주요 모델

| 모델 | 개발사 | 컨텍스트 | 비고 |
|------|--------|----------|------|
| `llama-3.3-70b-versatile` | Meta | 128K | 범용, 강력한 성능 |
| `llama-3.1-8b-instant` | Meta | 128K | 빠르고 경량 |
| `llama-3.2-11b-vision-preview` | Meta | 128K | 비전 + 텍스트 |
| `llama-3.2-90b-vision-preview` | Meta | 128K | 대형 비전 모델 |
| `deepseek-r1-distill-llama-70b` | DeepSeek | 128K | 추론 모델 (`reasoning_format` 지원) |
| `qwen-qwq-32b` | Alibaba | 128K | 추론 모델 |
| `mixtral-8x7b-32768` | Mistral | 32K | MoE 아키텍처 |
| `gemma2-9b-it` | Google | 8K | Google 오픈 모델 |
| `whisper-large-v3` | OpenAI | - | 오디오 트랜스크립션 전용 |

---

## 제한사항

| 파라미터 | 제한 |
|----------|------|
| `n` | **1만 지원** (다른 값은 400 에러) |
| `presence_penalty` | 미지원 |
| `logit_bias` | 미지원 |
| `logprobs` / `top_logprobs` | 미지원 |
| `audio` | 미지원 (TTS 없음, 트랜스크립션만 가능) |
| `web_search_options` | 미지원 |
| Embeddings API | 미지원 |

---

## 특수 파라미터

| 파라미터 | 설명 |
|----------|------|
| `reasoning_format` | 추론 모델용 (DeepSeek-R1, QwQ). 값: `"parsed"` (구조화), `"raw"` (`<think>` 태그 인라인), `"hidden"` (추론 수행하되 미반환) |
| `parallel_tool_calls` | 지원 (기본: `true`) |
| `stop` | 최대 4개 시퀀스 |

---

## IronHive 구현

### Config 클래스: `GroqConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |
| `ReasoningFormat` | `string?` | 추론 형식 (`"parsed"`, `"raw"`, `"hidden"`) |

### MessageGenerator: `GroqMessageGenerator`

- `CompatibleChatMessageGenerator` 기반

### 사용 예시

```csharp
builder.AddGroqProvider("groq", new GroqConfig
{
    ApiKey = "your-api-key"
});
```

---

## TODO

- [ ] PostProcess에서 `n=1` 강제, 미지원 파라미터 제거
- [ ] `reasoning_format` 파라미터 실제 요청 주입
- [ ] Responses API (Beta) 지원 검토

---

*Last Updated: 2026-02-06*
