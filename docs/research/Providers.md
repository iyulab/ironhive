각 Provider별 FinishReason 과 Error Debug 코드 정리

### FinishReason

- **OpenAI**
  - stop: 정상 종료
  - length: 토큰 초과
  - content_filter: 부적절한 컨텐츠 생성
  - tool_calls: 툴호출
- **Anthropic**
  - end_turn: 정상 종료
  - max_tokens: 토큰 초과
  - stop_sequence: 정지 시퀀스
  - tool_use: 툴호출
  - pause_turn: 내부툴을 사용하여 장기간 멈춰있어야 할때
  - refusal: 거부
- **Grok**
  - stop: 정상종료 또는 정지 시퀀스
  - length: 토큰 초과
- **Gemini**
  - FINISH_REASON_UNSPECIFIED: 사용되지 않음
  - STOP: 정상 정지 또는 정지 시퀀스 
  - MAX_TOKENS:	최대 토큰 제한
  - SAFETY:	부적절한 컨텐츠 생성
  - RECITATION:	과도한 컨텐츠 재현(학습 데이터를 그대로 뱉을경우(저작권 이슈))
  - LANGUAGE: 지원하지 않는 언어 사용
  - OTHER: 이유모름
  - BLOCKLIST: 금지된 용어 생성
  - PROHIBITED_CONTENT: 금지된 컨텐츠 포함
  - SPII: 개인식별정보(SPII) 포함 생성
  - MALFORMED_FUNCTION_CALL: 잘못된 함수호출 생성 
  - IMAGE_SAFETY: 부적절한 이미지 생성

### Error Debug
- 400: BadRequest
- 401: Unauthorized(미인증)
- 403: 권한 없음, 금지
- 404: Not Found(엔드포인트 없음)
- 405: Method Not Allowed(지원하지 않는 Method 형식)
- 413: 요청크기가 너무 큼
- 415: Unsupported Media Type
- 422: Unprocessable Entity(body 규격이 맞지 않음)
- 429: 요청제한(요청을 너무 많이했음)
- 500: 내부서버에러
- 503: 서버 일시적인 사용불가(서버 다운)
- 504: 서버가 시간안에 처리불가
- 529: 서버 과부하
