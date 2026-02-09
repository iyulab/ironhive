# xAI (Grok) Provider 가이드

**공식 문서**: [x.ai Docs](https://docs.x.ai/docs/overview)
**API Reference**: [x.ai API](https://docs.x.ai/docs/api-reference)
**Base URL**: `https://api.x.ai/v1`
**최종 업데이트**: `2026-02-06`

---

## 1. 주요 호환 API

- **Responses API**
- **Models API**

---

## 2. Responses API 제한 사항

### 파라미터 미지원

| 구분 | 미지원 (Not Supported) | 비고 |
| --- | --- | --- |
| **요청(Request)** | `background`, `metadata`, `service_tier`, `truncation`, `instructions` | `instructions` 대신 `system` 또는 `developer` 역할 사용 |
| **응답(Response)** | `frequency_penalty`, `presence_penalty` | 텍스트 반복 제어 관련 파라미터 확인 필요 |
| **기능(Tools)** | `functions`, `web search`만 지원 | 기타 커스텀 도구는 현재 제한적임 |

### 주요 모델별 특이점

- **`grok-3-mini` 전용**: 사고의 깊이를 조절하는 `reasoning_effort` 파라미터는 현재 이 모델에서만 지원됩니다.
- **Includes 속성**: `includes` 내에서 지원되는 값은 `reasoning.encrypted_content`가 유일합니다.

### 메시지 구조(Role) 가이드

- **System/Developer Role**: `instructions` 파라미터가 없으므로 `system` 또는 `developer` 역할을 사용해야 합니다.
- **단일 메시지 원칙**: `system`/`developer` 메시지는 단 **하나**만 허용됩니다.
- **순서 보장**: 해당 메시지는 반드시 대화의 **첫 번째 메시지**(`index: 0`)여야 합니다.

---
