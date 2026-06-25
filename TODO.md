# TODO LIST

### Providers
- Google AI: generateContent → Interactions API 전환 대기 (Google.GenAI .NET SDK에 Interactions API 지원 추가 시 마이그레이션, [dotnet-genai#159](https://github.com/googleapis/dotnet-genai/issues/159) 추적)

### Messages
- 메시지 스트리밍 응답시 마지막 응답에 전체 메시지 출력 고려(like responses API)

### 그외
- migration google interative api
- fixed yield return when streaming is not started
- fixed streaming_message when tool_completed_message when tool_result finished not tool_call finished
- add feature Store and StoreId (when Google Interactive API established)
- add feature EnableSuggesion (Custom content) in message_request
- add feature GenerateBatchAsync, CountTokensAsync in message_generator
- consider add vllm package 
- builder pattern redesign (to callback?)
