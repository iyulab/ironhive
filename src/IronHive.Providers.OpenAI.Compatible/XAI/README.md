# xAI (Grok) Provider

**공식 문서**: https://docs.x.ai/docs/overview
**API Reference**: https://docs.x.ai/docs/api-reference
**Base URL**: `https://api.x.ai/v1`
**API 타입**: Responses API (기본)

---

## 호환성

- Chat Completions API
- Responses API (기본 사용)
- Models API

---

## 주요 모델

| 모델 | 설명 |
|------|------|
| `grok-3` | 플래그십 모델 (추론, 코딩 최강) |
| `grok-3-fast` | 속도 최적화 버전 |
| `grok-3-mini` | 경량 추론 모델 (thinking/CoT 지원) |
| `grok-3-mini-fast` | 가장 빠른 Grok-3 변형 |
| `grok-2-vision` | 멀티모달 (텍스트 + 이미지 이해) |

---

## 특수 기능

### Responses API 파라미터

| 파라미터 | 설명 |
|----------|------|
| `reasoning` | 추론 설정 객체 (`effort`: minimal/low/medium/high, `summary`: auto/concise/detailed) |
| `store` | 생성 결과 xAI 서버 저장 여부 (xAI 기본값: `true`) |
| `previous_response_id` | 이전 응답 ID로 대화 연속성 유지 (전체 히스토리 재전송 불필요) |
| `background` | 백그라운드 작업 실행 (`store=true` 강제) |

### Chat Completions API 파라미터

| 파라미터 | 설명 |
|----------|------|
| `reasoning_effort` | 추론 모델용 (`low`, `medium`, `high`) |
| `web_search_options` | 웹 검색 설정 (`search_context_size`: low/medium/high) |

### Server-side Tools (Responses API)

- `web_search`: 실시간 웹 검색 및 페이지 브라우징 (`search_context_size`, `user_location` 설정 가능)
- `x_search`: X/Twitter 게시물, 사용자, 스레드 검색 (xAI 전용)
- `code_execution`: Python 코드 실행 (샌드박스 환경)

---

## IronHive 구현

### Config 클래스: `XAIConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |
| `EnableSearch` | `bool` | 실시간 웹 검색 활성화 |
| `SearchParameters` | `XAISearchParameters?` | 검색 파라미터 설정 |
| `Store` | `bool?` | 생성 결과 xAI 서버 저장 여부 |
| `PreviousResponseId` | `string?` | 이전 응답 ID로 대화 연속 |

### MessageGenerator: `XAIMessageGenerator`

- `CompatibleResponseMessageGenerator` 기반 (Responses API 사용)
- `PostProcessRequest`에서 Store, PreviousResponseId, 웹 검색 도구 주입 구현됨

### 사용 예시

```csharp
builder.AddxAIProvider("xai", new XAIConfig
{
    ApiKey = "your-api-key",
    EnableSearch = true,
    SearchParameters = new XAISearchParameters
    {
        MaxResults = 5,
        IncludeCitations = true
    }
});
```

---

## TODO

- [ ] `x_search`, `code_execution` Server-side Tool 지원 추가
- [ ] `reasoning` 파라미터 (effort, summary) 지원

---

*Last Updated: 2026-02-06*
