# ChatCompletion (내부 구현)

OpenAI Chat Completions API(`POST /chat/completions`)를 raw HTTP/JSON으로 구현한 내부 모듈입니다.
Ollama, LM Studio, vLLM, llama.cpp server, GPUStack 등 OpenAI 호환 서버 대상이며,
`OpenAICompatibleMessageGenerator`/`GpuStack.GpuStackMessageGenerator`가 내부적으로 위임합니다.

## 파일 구성

| 파일 | 역할 |
|---|---|
| `ChatCompletionMessageGenerator.cs` | `IMessageGenerator` 구현체 |
| `ChatCompletionHttpClient.cs` | raw HTTP client (SSE 스트리밍 파싱 포함) |
| `ChatCompletionPayloads.cs` | 요청/응답 DTO |
| `ChatCompletionJsonConverters.cs` | `ChatMessageContentJsonConverter`, `ExtraBodyJsonConverter`(+factory) |

## 왜 OpenAI SDK를 안 쓰는가

Ollama/vLLM류 reasoning 모델은 응답에 `reasoning_content`(또는 `reasoning`) 필드를 얹어 내려주는데, OpenAI SDK의
타입 모델엔 이 필드를 담을 자리가 없고 스트리밍에서는 raw JSON 접근 방법도 없습니다
([openai-dotnet#813](https://github.com/openai/openai-dotnet/issues/813) 미해결). 그래서 SDK 없이 JSON을 직접 다룹니다.

## ExtraBody

`ChatCompletionPayloadBase.ExtraBody`는 쓰기 시 루트 JSON에 deep merge(reasoning 모델용 vLLM 확장 파라미터
`thinking_token_budget`/`chat_template_kwargs`), 읽기 시 타입 모델이 모르는 프로퍼티를 원 위치 그대로 수집
(`choices[0].message.reasoning_content` 추출용)합니다.

`[JsonConverter]` 어트리뷰트를 타입에 직접 붙이지 않고 `ExtraBodyJsonConverterFactory`를
`JsonSerializerOptions.Converters`에 등록하는 이유: 컨버터 내부의 "알려진 프로퍼티는 기본 직렬화에 위임" 로직이
성립하려면 컨버터가 옵션 리스트를 통해서만 등록되어야 합니다. 타입 레벨 attribute는 어떤 옵션을 넘겨도 항상 우선
적용되어 내부 위임 호출이 자기 자신을 다시 불러 무한 재귀에 빠집니다(실측 확인됨).

## 스트리밍 컨텐츠 순서

thinking과 text는 하나의 "primary slot"을 공유합니다 — 다른 타입의 델타가 들어오면 즉시 현재 블록을 `Completed`로
닫고 새 블록을 엽니다(reasoning 모델은 추론과 답변이 스트림 중 섞이지 않으므로). 툴 콜은 `toolIndexMap`으로 독립
인덱스를 유지해 병렬 툴 콜이 뒤섞여 도착해도 서로 안 닫습니다.

## GPUStack: 스트리밍 에러 라인

GPUStack은 스트리밍 에러를 HTTP 에러가 아니라 `error: <message>` bare SSE 라인으로 내려줍니다
(`ChatCompletionHttpClient.PostStreamingAsync`에서 처리).

## 토큰 카운트

`CountTokensAsync`는 기본적으로 `NotSupportedException`을 던집니다. 호환 서버들은 서로 다른 토크나이저를 쓰는
임의의 모델을 서빙하므로(cl100k_base 등은 안 맞음), 정확한 카운트가 필요하면 서버별로 오버라이드하세요:

| | vLLM | llama.cpp server |
|---|---|---|
| 경로 | `POST /tokenize` | `POST /tokenize` |
| 요청 | `prompt` 또는 `messages`(+`tools`) | `content`(문자열만) |
| 응답 | `{ count, tokens, ... }` | `{ tokens: int[] }` (count 없음) |
