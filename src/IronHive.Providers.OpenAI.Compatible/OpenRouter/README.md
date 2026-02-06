# OpenRouter Provider

**공식 문서**: https://openrouter.ai/docs/api/reference/overview
**Base URL**: `https://openrouter.ai/api/v1`
**API 타입**: Chat Completions API

---

## 호환성

- Chat Completions API (주요 엔드포인트)
- Responses API
- Embeddings API (모델별 가용성 상이)
- Models API

---

## 모델 명명 규칙

OpenRouter는 `provider/model-name` 형식을 사용합니다:
- `openai/gpt-4o`
- `anthropic/claude-3.5-sonnet`
- `google/gemini-pro-1.5`
- `meta-llama/llama-3.1-405b-instruct`

---

## 특수 헤더

| 헤더 | 설명 |
|------|------|
| `HTTP-Referer` | 앱 URL (리더보드 식별용) |
| `X-Title` | 앱 이름 (리더보드 표시용) |

> **참고**: localhost URL은 반드시 `X-Title` 필요

---

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

| 필드 | 타입 | 설명 |
|------|------|------|
| `allow_fallbacks` | `bool` | 다른 Provider로 폴백 허용 여부 (기본: `true`) |
| `require_parameters` | `bool` | 요청한 파라미터를 모두 지원하는 Provider만 사용 (기본: `false`) |
| `order` | `string[]` | Provider 우선순위 목록 |
| `ignore` | `string[]` | 사용하지 않을 Provider 목록 |
| `quantizations` | `string[]` | 선호 양자화 레벨 (`fp32`, `fp16`, `bf16`, `int8`, `int4`) |
| `data_collection` | `string` | `"allow"` 또는 `"deny"` (데이터 수집 제어) |
| `sort` | `string` | 정렬 기준: `"price"`, `"throughput"`, `"latency"` |

---

## 응답 특수 필드

- `native_finish_reason`: 원본 모델의 finish_reason (OpenRouter 정규화 이전)
- `model`: 실제 사용된 모델 ID (폴백 시 요청 모델과 다를 수 있음)
- Generation ID로 `GET /api/v1/generation?id={id}` 조회하면 비용/메타데이터 확인 가능

---

## IronHive 구현

### Config 클래스: `OpenRouterConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |
| `SiteUrl` | `string?` | 사이트 URL (HTTP-Referer 헤더) |
| `AppName` | `string?` | 앱 이름 (X-Title 헤더) |
| `Transforms` | `IList<string>?` | 프롬프트 변환 목록 |
| `Route` | `string?` | 라우팅 전략 |
| `ProviderPreferences` | `OpenRouterProviderPreferences?` | Provider 선호도 설정 |

### `OpenRouterProviderPreferences`

| 속성 | 타입 | 설명 |
|------|------|------|
| `AllowFallbacks` | `bool?` | 폴백 허용 여부 |
| `RequireParameters` | `bool?` | 파라미터 지원 필수 여부 |
| `Order` | `IList<string>?` | Provider 우선순위 |
| `Ignore` | `IList<string>?` | 제외할 Provider 목록 |
| `Quantizations` | `IList<string>?` | 선호 양자화 레벨 |
| `DataCollection` | `string?` | 데이터 수집 제어 |
| `Sort` | `string?` | 정렬 기준 |

### MessageGenerator: `OpenRouterMessageGenerator`

- `CompatibleChatMessageGenerator` 기반
- `ToOpenAI()`에서 `HTTP-Referer`, `X-Title` 헤더 자동 설정

### 사용 예시

```csharp
builder.AddOpenRouterProvider("openrouter", new OpenRouterConfig
{
    ApiKey = "your-api-key",
    SiteUrl = "https://myapp.com",
    AppName = "My Application",
    Route = "fallback",
    ProviderPreferences = new OpenRouterProviderPreferences
    {
        AllowFallbacks = true,
        Order = new List<string> { "OpenAI", "Anthropic" }
    }
});
```

---

## TODO

- [ ] PostProcess에서 `transforms`, `route`, `provider` 파라미터 실제 요청 주입
- [ ] `native_finish_reason` 응답 필드 추출

---

*Last Updated: 2026-02-06*
