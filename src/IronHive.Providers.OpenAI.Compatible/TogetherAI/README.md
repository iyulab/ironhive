# Together AI Provider

**공식 문서**: https://docs.together.ai/docs/openai-api-compatibility
**Base URL**: `https://api.together.xyz/v1`
**최근 업데이트**: 2026-02-06

---

## 호환성

- Chat Completions API
- Embeddings API
- Models API

## Chat Completion API 추가 파라미터

- `chat_template_kwargs`: `object` - Together AI의 Chat Templates에 전달할 추가 매개변수
- `safety_model`: `string` - 안전성 필터링에 사용할 모델 지정
- `compliance`: `string` - 규정 준수 수준 설정 (예: "standard", "strict")
- `context_length_exceeded_behavior`: `string` - 컨텍스트 길이 초과 시 동작 지정 (예: "truncate", "error")

- 응답 `warnings` 필드: 모델이 응답 생성 중에 발생한 경고를 포함하는 배열

