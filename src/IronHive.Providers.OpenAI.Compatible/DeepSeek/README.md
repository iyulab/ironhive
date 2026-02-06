# DeepSeek Provider

**공식 문서**: https://api-docs.deepseek.com/
**Base URL**: `https://api.deepseek.com/v1`
**API 타입**: Chat Completions API

> **참고**: Base URL은 `https://api.deepseek.com/v1` 또는 `https://api.deepseek.com` 모두 사용 가능합니다. `/v1`은 OpenAI SDK 호환을 위한 경로입니다.

---

## 호환성

- Chat Completions API
- Models API
- FIM Completions API (Beta, `/beta` base URL)

---

## 주요 모델

| 모델 | 설명 | 컨텍스트 | 출력 |
|------|------|----------|------|
| `deepseek-chat` | DeepSeek-V3 (범용) | 64K 입력 | 8K |
| `deepseek-reasoner` | DeepSeek-R1 (추론 특화) | 64K 입력 | 64K (CoT 포함) |

---

## 특수 기능

### Thinking Mode (추론 모드)

`deepseek-reasoner` 모델에서 사용하며, Chain-of-Thought 추론 과정을 노출합니다.

**요청 시 활성화:**
```json
{
  "model": "deepseek-reasoner",
  "messages": [...],
  "thinking": {
    "type": "enabled",
    "budget_tokens": 8192
  }
}
```

- `budget_tokens`: 추론에 사용할 최대 토큰 수 (최소 1024, 최대 65536). 생략 시 기본값 사용.

**응답 형식:**
```json
{
  "choices": [{
    "message": {
      "role": "assistant",
      "content": "최종 답변",
      "reasoning_content": "단계별 추론 과정..."
    }
  }]
}
```

**스트리밍**: `delta.reasoning_content`와 `delta.content`는 상호 배타적. 추론 청크가 먼저 모두 전송된 후 콘텐츠 청크가 전송됩니다.

**추론 모드에서 무시되는 파라미터:**
- `temperature`, `top_p`, `presence_penalty`, `frequency_penalty`

**주의사항:**
- 멀티턴 대화 시 이전 턴의 `reasoning_content`를 다시 전달하지 말 것 (무시되며 토큰 낭비)
- 추론 모드에서도 Tool calling 지원됨

### Prefix Completion (Beta)

```
Base URL: https://api.deepseek.com/beta
```

Assistant 메시지에 `"prefix": true` 설정:
```json
{"role": "assistant", "content": "```python\ndef sort_list(", "prefix": true}
```

모델이 프리픽스 이후부터 이어서 생성합니다.

### FIM (Fill-in-the-Middle) (Beta)

```
Endpoint: POST https://api.deepseek.com/beta/completions
```

특수 토큰 `<|fim_begin|>`, `<|fim_hole|>`, `<|fim_end|>`을 사용하여 코드 빈칸 채우기를 수행합니다.

---

## 제한사항

| 파라미터 | 제한 |
|----------|------|
| `n` | 1만 지원 |
| `logprobs` / `logit_bias` | 미지원 |
| Embeddings API | 미지원 |
| Responses API | 미지원 |

---

## IronHive 구현

### Config 클래스: `DeepSeekConfig`

| 속성 | 타입 | 설명 |
|------|------|------|
| `ApiKey` | `string?` | 인증용 API 키 |
| `EnableThinking` | `bool` | Thinking 모드 활성화 (deepseek-reasoner 전용) |
| `ThinkingBudgetTokens` | `int?` | 추론 최대 토큰 수 (1024~65536) |

### MessageGenerator: `DeepSeekMessageGenerator`

- `CompatibleChatMessageGenerator` 기반

### 사용 예시

```csharp
builder.AddDeepSeekProvider("deepseek", new DeepSeekConfig
{
    ApiKey = "your-api-key"
});
```

---

## TODO

- [ ] PostProcess에서 `thinking` 파라미터 주입
- [ ] `reasoning_content` 응답 추출
- [ ] Beta URL 전환 처리 (Prefix Completion, FIM)

---

*Last Updated: 2026-02-06*
