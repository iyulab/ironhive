# DeepSeek Provider

**공식 문서**: https://api-docs.deepseek.com/
**Base URL**: `https://api.deepseek.com/v1`
**최종 업데이트**: 2026-02-06

---

## 호환성

- Chat Completions API
- Models API

## Chat Completions API 특수 기능 및 제한

### Thinking Mode (추론 모드)

**요청 시 활성화:**
```json
{
  "model": "deepseek-reasoner",
  "messages": [...],
  "thinking": {
    "type": "enabled",
  }
}
```

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

### Not Supported Features
- temperature、top_p、presence_penalty、frequency_penalty、logprobs、top_logprobs. 
- Please note that to ensure compatibility with existing software, setting temperature、top_p、presence_penalty、frequency_penalty will not trigger an error but will also have no effect. Setting logprobs、top_logprobs will trigger an error.
