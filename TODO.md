# TODO LIST

### Providers
- OpenAI SDK로 업그레이드 보류(Response API에 ExtraBody기능 추가 될때까지)

### Messages
- 메시지 스트리밍 응답시 마지막 응답에 전체 메시지 출력 고려(like responses API)
- Count_Token 기능 추가(생성 직전 토큰 사용량 검증 로직에 필요)
- 각 provider들의 생성id를 이용한 cache hit 방식 적용
- 