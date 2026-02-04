# OpenAI Compatible Providers Research

## Overview

OpenAI API 형식과 호환되는 서비스 제공자들에 대한 조사 결과입니다. 각 제공자별 API 엔드포인트, 특수 기능, 제한사항을 정리합니다.

---

## Provider 목록

| Provider | Base URL | 주요 특징 |
|----------|----------|-----------|
| xAI (Grok) | `https://api.x.ai/v1` | 실시간 검색, X/Twitter 검색 |
| Groq | `https://api.groq.com/openai/v1` | 초고속 추론 |
| DeepSeek | `https://api.deepseek.com/v1` | Thinking 모드, Prefix Completion |
| Together AI | `https://api.together.xyz/v1` | 오픈소스 모델 호스팅 |
| Fireworks AI | `https://api.fireworks.ai/inference/v1` | 고성능 모델 서빙 |
| Perplexity | `https://api.perplexity.ai` | 검색 기반 AI |
| OpenRouter | `https://openrouter.ai/api/v1` | 다중 모델 라우터 |
| vLLM | 사용자 정의 (기본: `http://localhost:8000/v1`) | Self-hosted 추론 엔진 |
| GPUStack | 사용자 정의 | Self-hosted GPU 클러스터 |

---

## 상세 분석

### 1. xAI (Grok)

**공식 문서**: https://docs.x.ai/docs/overview

#### 호환성
- OpenAI SDK와 완전 호환
- Chat Completions API 및 Responses API 지원

#### 특수 기능

| 파라미터 | 설명 |
|----------|------|
| `reasoning_effort` | 추론 모델용 (`low`, `medium`, `high`) |
| `store` | 생성 결과 저장 여부 (기본: true) |
| `previous_response_id` | 대화 연속성 유지 |
| `search_enabled` | 실시간 웹 검색 활성화 |
| `search_parameters` | 검색 설정 (`max_search_results`, `include_citations`, `search_timeout`) |

#### Server-side Tools
- `web_search`: 실시간 웹 검색 및 페이지 브라우징
- `x_search`: X/Twitter 게시물, 사용자, 스레드 검색
- `code_execution`: Python 코드 실행

#### 인증
```
Authorization: Bearer <XAI_API_KEY>
```

---

### 2. Groq

**공식 문서**: https://console.groq.com/docs/overview

#### 호환성
- OpenAI API와 대부분 호환
- Responses API 베타 지원

#### 제한사항

| 파라미터 | 제한 |
|----------|------|
| `n` | **1만 지원** (다른 값은 400 에러) |
| `presence_penalty` | 미지원 |
| `logit_bias` | 미지원 |
| `logprobs` / `top_logprobs` | 미지원 |

#### Vision 모델 제한
- Image URL: 최대 20MB
- Base64 Encoded: 최대 4MB

#### 지원 파라미터
- `parallel_tool_calls`: 지원 (기본: true)
- `stop`: 최대 4개 시퀀스

#### 인증
```
Authorization: Bearer <GROQ_API_KEY>
```

---

### 3. DeepSeek

**공식 문서**: https://api-docs.deepseek.com/

#### 호환성
- OpenAI SDK 호환 (`base_url` 변경만 필요)
- `/v1` 또는 `/beta` 엔드포인트 사용

#### 모델
- `deepseek-chat`: 비추론 모드 (DeepSeek-V3.2)
- `deepseek-reasoner`: 추론 모드 (DeepSeek-V3.2)

#### 특수 기능

##### Thinking Mode (추론 모드)
```python
extra_body={"thinking": {"type": "enabled"}}
```

응답에 `reasoning_content` 필드가 포함됨 (CoT 내용)

**주의**: 추론 모드에서 무시되는 파라미터:
- `temperature`
- `top_p`
- `presence_penalty`
- `frequency_penalty`

##### Prefix Completion (Beta)
```
Base URL: https://api.deepseek.com/beta
```

Assistant 메시지에 `"prefix": true` 설정:
```json
{"role": "assistant", "content": "```python\n", "prefix": true}
```

#### 인증
```
Authorization: Bearer <DEEPSEEK_API_KEY>
```

---

### 4. OpenRouter

**공식 문서**: https://openrouter.ai/docs/api/reference/overview

#### 호환성
- OpenAI Chat API와 유사한 스키마
- `choices`는 항상 배열
- `finish_reason` 정규화: `tool_calls`, `stop`, `length`, `content_filter`, `error`

#### 특수 헤더

| 헤더 | 설명 |
|------|------|
| `HTTP-Referer` | 앱 URL (리더보드 식별용) |
| `X-Title` | 앱 이름 (리더보드 표시용) |

**참고**: localhost URL은 반드시 X-Title 필요

#### 특수 파라미터

| 파라미터 | 설명 |
|----------|------|
| `transforms` | 프롬프트 변환 배열 |
| `route` | 라우팅 전략 (예: `fallback`) |
| `provider` | Provider 선호도 설정 |

##### Provider Preferences
```json
{
  "provider": {
    "allow_fallbacks": true,
    "require_parameters": false,
    "order": ["openai", "anthropic"]
  }
}
```

#### 응답 특수 필드
- `native_finish_reason`: 원본 모델의 finish_reason

#### 인증
```
Authorization: Bearer <OPENROUTER_API_KEY>
```

---

### 5. Together AI

**공식 문서**: https://docs.together.ai/docs/openai-api-compatibility

#### 호환성
- OpenAI SDK와 **완전 호환**
- Chat, Vision, Images, Embeddings, Speech 지원

#### 설정
```python
client = openai.OpenAI(
    api_key=os.environ.get("TOGETHER_API_KEY"),
    base_url="https://api.together.xyz/v1",
)
```

#### 인증
```
Authorization: Bearer <TOGETHER_API_KEY>
```

---

### 6. Fireworks AI

**공식 문서**: https://docs.fireworks.ai/

#### 호환성
- OpenAI API 호환
- 고성능 모델 서빙에 최적화

#### 인증
```
Authorization: Bearer <FIREWORKS_API_KEY>
```

---

### 7. Perplexity

**공식 문서**: https://docs.perplexity.ai/

#### 호환성
- OpenAI Chat API 호환
- 검색 기반 응답이 기본 내장

#### 모델
- `sonar-deep-research`
- `sonar-reasoning-pro`
- `sonar-reasoning`
- `sonar-pro`
- `sonar`
- `r1-1776`

#### 제한사항
- `logit_bias`, `logprobs`, `top_logprobs`: 미지원
- `n`: 1만 지원 가능성

#### 인증
```
Authorization: Bearer <PERPLEXITY_API_KEY>
```

---

### 8. vLLM

**공식 문서**: https://docs.vllm.ai/en/stable/serving/openai_compatible_server/

#### 개요
- 고처리량 오픈소스 LLM 서빙 엔진
- 메모리 최적화 및 처리 속도 최적화

#### 서버 실행
```bash
vllm serve <model-name>
# 기본 포트: 8000
```

#### 엔드포인트
- `http://localhost:8000/v1`

#### 호환성
- OpenAI Completions API
- OpenAI Chat API

---

### 9. GPUStack

**공식 문서**: https://github.com/gpustack/gpustack

#### 개요
- Self-hosted GPU 클러스터 관리 플랫폼
- OpenAI 호환 API 제공

#### 특이사항
- 스트리밍 응답에서 `error:` 프리픽스로 에러 전달

---

## 공통 호환성 이슈

### 1. 파라미터 지원 차이

| 파라미터 | OpenAI | Groq | DeepSeek (reasoning) | 비고 |
|----------|--------|------|---------------------|------|
| `n` | O | 1만 | O | |
| `presence_penalty` | O | X | 무시 | |
| `frequency_penalty` | O | O | 무시 | |
| `temperature` | O | O | 무시 | |
| `top_p` | O | O | 무시 | |
| `logit_bias` | O | X | O | |
| `logprobs` | O | X | O | |

### 2. Tool Calling

대부분의 Provider가 OpenAI 형식의 Tool Calling 지원:
```json
{
  "tools": [{
    "type": "function",
    "function": {
      "name": "get_weather",
      "description": "Get weather",
      "parameters": {...}
    }
  }],
  "tool_choice": "auto"
}
```

### 3. 스트리밍

모든 Provider가 SSE (Server-Sent Events) 스트리밍 지원:
```
data: {"choices": [...]}
data: {"choices": [...]}
data: [DONE]
```

---

## 참고 자료

- [xAI API Reference](https://docs.x.ai/docs/api-reference)
- [Groq API Reference](https://console.groq.com/docs/api-reference)
- [DeepSeek API Docs](https://api-docs.deepseek.com/)
- [OpenRouter Documentation](https://openrouter.ai/docs)
- [Together AI Docs](https://docs.together.ai/)
- [Fireworks AI Docs](https://docs.fireworks.ai/)
- [Perplexity API](https://docs.perplexity.ai/)
- [vLLM Documentation](https://docs.vllm.ai/)
- [AI SDK OpenAI Compatible Providers](https://ai-sdk.dev/providers/openai-compatible-providers)
- [LiteLLM Providers](https://docs.litellm.ai/docs/providers)
