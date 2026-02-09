# OpenRouter Provider

**공식 문서**: https://openrouter.ai/docs/api/reference/overview
**Base URL**: `https://openrouter.ai/api/v1`
**최근 업데이트**: 2026-02-06

---

## 호환성

- Responses API (Experimental)
- Embeddings API
- Models API

## 모델 명명 규칙

OpenRouter는 `provider/{model_name}` 형식을 사용합니다:
- `openai/gpt-4o`
- `anthropic/claude-3.5-sonnet`
- `google/gemini-pro-1.5`
- `meta-llama/llama-3.1-405b-instruct`

## 특수 헤더

| 헤더 | 설명 |
|------|------|
| `HTTP-Referer` | 앱 URL (리더보드 식별용) |
| `X-Title` | 앱 이름 (리더보드 표시용) |

## 특수 파라미터

| 파라미터 | 타입 | 설명 |
|----------|------|------|
| `transforms` | `string[]` | 프롬프트 변환 배열 (예: `["middle-out"]` -- 긴 프롬프트 자동 압축) |
| `route` | `string` | 라우팅 전략 (예: `"fallback"` -- 실패 시 다른 Provider 시도) |
| `models` | `string[]` | 폴백 모델 목록 (기본 모델 불가 시 순서대로 시도) |
| `provider` | `object` | Provider 선호도 설정 (아래 상세) |

### Provider Preferences

```json
{
  "provider": {
    "allow_fallbacks": true,
    "require_parameters": false,
    "order": ["OpenAI", "Anthropic"],
    "ignore": ["Together"],
    "quantizations": ["fp16", "bf16"],
    "data_collection": "deny",
    "sort": "price"
  }
}
```

---

## 응답 특수 필드

- `native_finish_reason`: 원본 모델의 finish_reason (OpenRouter 정규화 이전)
- `model`: 실제 사용된 모델 ID (폴백 시 요청 모델과 다를 수 있음)
- Generation ID로 `GET /api/v1/generation?id={id}` 조회하면 비용/메타데이터 확인 가능