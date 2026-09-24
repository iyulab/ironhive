# AI 프로바이더

IronHive에서 지원하는 AI 프로바이더별 구현과 설정 방법입니다.

## 개요

각 프로바이더는 `IMessageGenerator`, `IEmbeddingGenerator`, `IImageGenerator` 등 동일 인터페이스를 구현하여 코드 변경 없이 교체 가능합니다. 등록 시 부여한 이름(string key)으로 라우팅됩니다.

---

## OpenAI

**패키지**: `IronHive.Providers.OpenAI`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `OpenAIMessageGenerator` | Responses API(`/v1/responses`) 구현. reasoning summary/encrypted content 지원 |
| `OpenAIEmbeddingGenerator` | 텍스트 임베딩 |
| `OpenAIImageGenerator` | DALL-E 이미지 생성/편집 |
| `OpenAIAudioProcessor` | TTS (tts-1, tts-1-hd) + STT (Whisper) |
| `OpenAIModelFinder` | 사용 가능한 모델 목록 조회 |

`IronHive.Providers.OpenAI`는 **Responses API**(`POST /v1/responses`)만 사용한다. Chat Completions(`POST /v1/chat/completions`) — 아래 `IronHive.Providers.OpenAI.Compatible` 패키지를 사용한다.

### 등록

```csharp
// 모든 서비스 한 번에 등록
builder.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKey = "sk-..."
});

// 일부만 등록
builder.AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "..." },
    OpenAIServiceType.Messages | OpenAIServiceType.Embeddings);

// 개별 등록
builder.AddMessageGenerator("openai", new OpenAIMessageGenerator(config));
builder.AddEmbeddingGenerator("openai", new OpenAIEmbeddingGenerator(config));
```

### OpenAIConfig

```csharp
public class OpenAIConfig
{
    public string ApiKey { get; set; }          // 필수는 아니다 — 아래 참조
    public string BaseUrl { get; set; }         // 커스텀 엔드포인트·게이트웨이용. 버전 세그먼트 포함
    public string Organization { get; set; }    // 조직 ID (옵션)
    public string Project { get; set; }         // 프로젝트 ID (옵션)
    public TimeSpan Timeout { get; set; }        // 요청 타임아웃. 기본 Timeout.InfiniteTimeSpan(무제한)
    public TimeSpan ConnectTimeout { get; set; } // TCP 연결 타임아웃. 기본 5초
    public HttpClient? HttpClient { get; set; }
    public IDictionary<string, string>? Headers { get; set; }   // 게이트웨이 헤더 — 「추가 요청 헤더」 절
}
```

`HttpClient`를 직접 주입하지 않으면 어댑터가 `ConnectTimeout`을 적용하고
`HttpClient.Timeout`을 무제한으로 설정한 기본 클라이언트를 만든다 — 응답이 느린 요청은 `Timeout`을
명시적으로 설정하지 않는 한 무한정 기다리고, 대신 연결 자체가 안 되는 호스트는 `ConnectTimeout` 안에
빠르게 실패한다.

#### `BaseUrl`은 버전 세그먼트를 포함한 완전한 엔드포인트다

이 값은 벤더 SDK 엔드포인트로 그대로 전달되고 **어댑터는 `/v1`을 붙이지 않는다.**
`https://gateway.example.com`을 넣으면 요청이 `/responses`·`/models`로 나가 404가 된다 —
올바른 값은 `https://gateway.example.com/v1`이다.

⚠️ 아래 `OpenAICompatibleConfig.BaseUrl`은 **같은 이름에 반대 계약**이다: 그쪽은 «API 경로 없는 서버
주소»이고 `Path`(기본 `/v1`)를 어댑터가 덧붙인다. 두 설정 사이에서 같은 값을 그대로 옮기면 한쪽이
404가 된다. 경로를 자동으로 붙이지 않는 이유는 규칙이 호환 서비스마다 다르기 때문이다
(GPUStack은 `/v1-openai`). 이 계약은 `BaseUrlPathContractTests`가 와이어 레벨로 고정한다.

#### `ApiKey`가 없어도 등록·요청이 가능하다

자격증명을 요구하지 않는 엔드포인트(호환 로컬 서버, 또는 상류에서 자격증명을 주입하는 게이트웨이)를
위해 키가 비면 placeholder 자격증명이 쓰인다. 실제 OpenAI를 상대로 키를 빠뜨리면 요청 시점에 인증
오류로 드러난다. `OpenAIConfig.Validate()`는 «키가 있는가»에만 답하며 등록 게이트가 아니다.

### 모델 세대별 능력 정책

`reasoning.effort` 가 받는 값은 모델마다 다르고 범위 밖 값은 400(«Unsupported value») 이다.
`OpenAIModelCapabilities.ReasoningEfforts` 가 모델(가장 긴 접두 일치)별 허용 값을 갖고, `OpenAIConfig.ModelCapabilities` 로
덮어쓴다. 요청의 노력도는 받는 값이면 그대로, 아니면 가장 가까운 값(`none` 제외, 동률이면 높은 쪽)으로 간다.
`MessageThinkingEffort.None` 은 «꺼 달라» — `none`, 없으면 그 모델의 최저 값.

| 모델 | `ReasoningEfforts` | 예 |
|---|---|---|
| gpt-4o · gpt-4.1 | `null` — `reasoning` 을 보내지 않음 | 어떤 노력도든 미전송 |
| gpt-5 · -mini · -nano | minimal · low · medium · high | None → `minimal` · XHigh → `high` |
| gpt-5.1 | none · low · medium · high | Minimal → `low` · XHigh → `high` |
| gpt-5.x (기본 · 모르는 모델) | none · low · medium · high · xhigh | Minimal → `low` |
| gpt-5.6 | none · … · xhigh · max | |
| o1 · o3 · o4 | low · medium · high | None → `low` · XHigh → `high` |

표는 2026-09-17 `POST /v1/responses` 값 검증 오류 원문으로 실측했다.

### 지원 기능

- Responses API
- Function Calling (도구 호출)
- 스트리밍
- 추론 노력도 (`ThinkingEffort`) 설정
- 임베딩: `text-embedding-3-small`, `text-embedding-3-large`, `text-embedding-ada-002`
- 이미지: DALL-E 2/3 생성 + 편집
- 오디오: TTS (6개 음성) + Whisper STT

### ServiceType Flags

```csharp
[Flags]
public enum OpenAIServiceType
{
    Models = 1,
    Messages = 2,
    Embeddings = 4,
    Images = 8,
    Audio = 16,
    All = Models | Messages | Embeddings | Images | Audio
}
```

---

## Anthropic

**패키지**: `IronHive.Providers.Anthropic`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `AnthropicMessageGenerator` | Claude 모델 메시지 생성 |
| `AnthropicModelFinder` | Claude 모델 목록 |

### 등록

```csharp
builder.AddAnthropicProviders("anthropic", new AnthropicConfig
{
    ApiKey = "sk-ant-..."
});
```

### AnthropicConfig

```csharp
public class AnthropicConfig
{
    public string? ApiKey { get; set; }          // API 키 또는 AuthToken 중 하나 필수
    public string? AuthToken { get; set; }
    public string? BaseUrl { get; set; }         // 완전한 엔드포인트 (벤더 기본값: https://api.anthropic.com)
    public IDictionary<string, string>? ExtraHeaders { get; set; }
    public int? MaxRetries { get; set; }
    public TimeSpan Timeout { get; set; }        // 요청 타임아웃. 기본 Timeout.InfiniteTimeSpan(무제한)
    public TimeSpan ConnectTimeout { get; set; } // TCP 연결 타임아웃. 기본 5초
    public HttpClient? HttpClient { get; set; }
    public IDictionary<string, string>? Headers { get; set; }   // ExtraHeaders와 같은 슬롯(합집합) — 「추가 요청 헤더」 절
    public IDictionary<string, AnthropicModelCapabilities>? ModelCapabilities { get; set; } // 모델 세대별 능력 정책 덮어쓰기
}
```

### 모델 세대별 능력 정책

생성기는 「요청된 의도」를 「대상 모델이 받는 wire」로 번역한다. 무엇을 받는지는 세대마다 다르고 그 규칙은
vendor 지식이라 provider 안의 `AnthropicModelCapabilities`가 갖는다 — 내장 표는 모델 id 의 정확 일치 →
가장 긴 접두 일치로 찾고, 없는 모델은 최신 세대와 같다고 본다.

| 세대 | `ThinkingStyle` | `SupportsForcedToolChoice` | 번역 |
|---|---|---|---|
| Claude 4.x (`claude-sonnet-4-5` 등) | `Budget` — `thinking: {type: enabled, budget_tokens}` | `true` | `RequiredToolChoice` → `tool_choice: any` |
| Claude 5.1 (`claude-fable-5-1`, `claude-mythos-5-1`) · Opus 5.5 (`claude-opus-5-5`) | `Adaptive` | **`false`** — `any`/`tool`은 400 | `tool_choice: auto` + 시스템 프롬프트 끝에 도구 호출 지시(vendor 마이그레이션 가이드의 처방) |

`Adaptive` 세대는 요청의 노력도를 `output_config.effort` 로 싣는다(Minimal·Low → `low` · Medium → `medium` · High →
`high` · XHigh → `xhigh`, `SupportsXHighEffort=false`(4.6 세대)는 `high`). `MessageThinkingEffort.None` 은 «꺼 달라»다 —
Claude Sonnet 5 · Opus 5 · Fable 은 `thinking` 을 생략해도 생각한다. `SupportsDisabledThinking=true`(Sonnet 5 · Opus
4.6~4.8)는 `thinking: {type: "disabled"}`, 아니면(Fable · Opus 5.5: disabled 는 400 · Opus 5: vendor 가 끄기 대신 낮은 effort 를 권함 ·
모르는 모델) `output_config.effort: low` 를 보낸다. `Budget` 세대는 생략이 곧 off 이고 `effort` 를 받지 않는다(Haiku 4.5 는
400) — 아무것도 보내지 않는다. 2026-09-17 실키 실측(sonnet-5 · opus-5 · haiku-4-5, None/Low/XHigh 전부 400 없음).
2026-09-24 실키 실측(opus-5-5): `tool_choice: any` 400 · `thinking: disabled` 400 · `effort: low` 정상.

노력도를 지정하지 않으면(`ThinkingEffort = null`) `output_config.effort` 를 보내지 않으므로 vendor 기본값이 적용된다 —
Opus 5.5 의 기본값은 `medium` 으로 Opus 5(`high`)보다 한 단계 낮다. 모델 id 만 바꿔 옮기는 호출자는 생각 깊이가 조용히
줄어드니, 이전과 같은 깊이가 필요하면 `ThinkingEffort` 를 명시한다.

`Budget` 세대의 예산(Minimal 1,024 · Low 4,000 · Medium 10,000 · High 20,000 · XHigh 32,000)은 `max_tokens` 보다
작아야 한다(아니면 400 «`max_tokens` must be greater than `thinking.budget_tokens`»). 호출자의 `MaxTokens` 가 예산
이하이면 예산을 그 절반으로 줄여 답변 자리를 남기고, 절반이 vendor 최소 예산 1,024 에 못 미치면 thinking 을 켜지
않는다. `MaxTokens` 를 주지 않으면 `max_tokens` 는 64,000 이라 조정되지 않는다.

내장 표에 없는 새 모델은 `ModelCapabilities`로 코드 수정 없이 선언한다. 소비자 항목이 내장 표보다 우선한다.

```csharp
new AnthropicConfig
{
    ApiKey = "sk-ant-...",
    ModelCapabilities = new Dictionary<string, AnthropicModelCapabilities>
    {
        ["claude-fable-5-2"] = new() { SupportsForcedToolChoice = false },   // 접두 일치 — 날짜 접미사 포함
    }
}
```

`HttpClient`를 직접 설정하지 않으면 어댑터가 `ConnectTimeout`을 적용하고 `HttpClient.Timeout`을
무제한으로 설정한 기본 클라이언트를 만든다 — 연결 자체가 안 되는 호스트만 `ConnectTimeout` 안에
빠르게 실패하고, 응답이 느린 요청은 `Timeout`을 명시적으로 설정하지 않는 한 무한정 기다린다.

### 지원 기능

- Messages API
- Function Calling
- 스트리밍
- Extended Thinking (`ThinkingContent`) — Claude 3.7+
- 멀티모달 입력 (이미지)
- 시스템 프롬프트

### ServiceType Flags

```csharp
[Flags]
public enum AnthropicServiceType
{
    Models = 1,
    Messages = 2,
    All = Models | Messages
}
```

---

## Google AI (Gemini)

**패키지**: `IronHive.Providers.GoogleAI`

### 구성 요소

| 클래스 | 설명 |
|--------|------|
| `GoogleAIMessageGenerator` | Gemini 메시지 생성 |
| `GoogleAIEmbeddingGenerator` | Gemini 임베딩 |
| `GoogleAIImageGenerator` | Imagen 이미지 생성 |
| `GoogleAIVideoGenerator` | Veo 비디오 생성 |
| `GoogleAIAudioProcessor` | TTS/STT (GenerateContent 기반) |
| `GoogleAIModelFinder` | Gemini 모델 목록 |

### 등록

```csharp
// Google AI Studio
builder.AddGoogleAIProviders("google", new GoogleAIConfig
{
    ApiKey = "AQ....",   // AI Studio auth key (서비스 계정 바인딩) — 2026-09부터 표준 키(AIza…)는 거부된다
    Timeout = TimeSpan.FromMinutes(10)   // 생략 시 무제한 (ConnectTimeout만 적용)
});

// Vertex AI
builder.AddVertexAIProviders("vertex", new VertexAIConfig
{
    Project = "my-project",
    Location = "us-central1"
    // 자격증명은 Application Default Credentials 사용
});
```

> **키 형식**: Gemini API는 2026-09부터 «표준» API 키(`AIza…`)를 전부 거부하고, Google Cloud 서비스 계정에
> 바인딩된 **auth key**(`AQ.` 접두 — AI Studio에서 새로 발급하면 기본값)만 받는다. 2026년 중반 이전에 만든 키가
> `401`/`403`을 내면 AI Studio API Keys 페이지에서 «Standard»인지 확인하고 재발급한다. 라이브러리는 키 형식을
> 검사하지 않는다 — 값을 그대로 전달한다.

### 타임아웃

`GoogleAIConfig.Timeout` / `VertexAIConfig.Timeout`이 요청 타임아웃을 정한다. **생략하면 요청
타임아웃을 두지 않는다** — 대신 `ConnectTimeout`(기본 5초)이 TCP 연결 수립을 제한하므로, 응답이
없는 호스트에서 무한정 멈추지는 않는다. 어댑터는 `HttpClientFactory`를 지정하지 않으면 항상 자체
`HttpClient`를 공급해 `Timeout`을 무제한으로 두므로, 벤더 SDK가 만드는 바닐라 `HttpClient`의 100초
기본값을 조용히 물려받는 일이 없다.

`HttpOptions`로도 같은 값을 지정할 수 있으나(밀리초 단위) **둘을 동시에 설정하면
`InvalidOperationException`을 던진다.** 어느 쪽이 이겼는지 알 수 없는 상태를 만들지 않기 위한 것이며,
`HttpOptions`는 `BaseUrl` 등 나머지 설정에 계속 쓸 수 있다.

`GoogleAIConfig.Headers` / `VertexAIConfig.Headers`는 벤더 `HttpOptions.Headers`에 병합된다(둘 다 설정 가능, 같은 이름은 값이 같아야 한다) — 「추가 요청 헤더」 절.

### 모델 세대별 능력 정책

Anthropic 과 같은 형태로 `GoogleAIModelCapabilities`가 세대별 wire 규칙을 갖는다(정확 일치 → 가장 긴 접두
일치, 없는 모델은 최신 세대). `GoogleAIConfig.ModelCapabilities` / `VertexAIConfig.ModelCapabilities`로
덮어쓴다.

| 세대 | `ThinkingControl` | `SupportsMinimalThinking` | `SupportsZeroThinkingBudget` | `SupportsSamplingParameters` | `SupportsMultimodalFunctionResponse` |
|---|---|---|---|---|---|
| Gemini 1.5 / 2.0 | `None` — thinking 파라미터 없음 | — | — | `true` | **`false`** — 이미지/오디오 도구 결과는 텍스트 자리표시자로 |
| Gemini 2.5 | `Budget` — `thinkingBudget`(Minimal 1,024 · Low 4,000 · Medium 10,000 · High 20,000 · XHigh 24,576) | — | `true` | `true` | **`false`** — `inlineData` 는 `400 Multimodal function responses are not supported` |
| Gemini 2.5 Pro | `Budget` | — | **`false`** — 끌 수 없음 → 최소 예산 128 | `true` | **`false`** |
| Gemini 3 (기본) | `Level` — `thinkingLevel` | `true` | `false` | `true` | `true` — `functionResponse.parts[].inlineData` |
| Gemini 3 Pro · 3.1 Pro | `Level` | **`false`** — `low`/`high` 만 | `false` — thinking 모드에서만 동작 | `true` | `true` |
| Gemini 3.6 Flash | `Level` | `true` | **`true`** | `true` | `true` |
| Gemini 3.7 Flash | `Level` | **`false`** | **`true`** | `true` | `true` |
| Gemini 3.8 Flash | `Level` | **`false`** — `minimal`은 오류 → `low`로 강등 | **`true`** | **`false`** — `temperature`/`topP`/`topK`를 보내지 않음 | `true` |

**`MessageThinkingEffort.None`(= `ChatOptions.Reasoning.Effort = None`)은 «보내지 않음»이 아니라 «꺼 달라»다.**
Gemini 2.5·3 계열 대부분은 기본으로 생각하고 thinking 토큰이 `maxOutputTokens` 에 포함되므로, 끄지 않으면 짧은
응답이 빈 문자열(`finishReason: MAX_TOKENS`)로 돌아온다. `SupportsZeroThinkingBudget` 이 `true` 인 모델에는
`thinkingBudget: 0` 을, 아닌 모델에는 받는 가장 낮은 단계(`minimal`, 안 받으면 `low` · Budget 모델은 128)를
보낸다 — 끌 수 없는 모델(Pro)은 여전히 생각하므로 출력 예산을 넉넉히 준다. 모르는 모델의 기본값이 `false` 인 것은
예산 0 을 거부하는 모델(3.1 Pro · 3.5 Flash-Lite)에서 호출 자체가 400 이 되기 때문이다. 표의 끄기·`minimal` 행은
2026-09-17 실키 실측.

### 지원 기능

- Generate Content API
- Function Calling
- 스트리밍
- 멀티모달: 텍스트, 이미지, 비디오, 오디오, 문서
- 임베딩: `text-embedding-004` 등
- 이미지 생성: `imagen-3.0` 등
- 비디오 생성: `veo-2.0` 등 (비동기 폴링 방식)

### ServiceType Flags

```csharp
[Flags]
public enum GoogleAIServiceType
{
    Models = 1,
    Messages = 2,
    Embeddings = 4,
    Images = 8,
    Videos = 16,
    Audio = 32,
    All = Models | Messages | Embeddings | Images | Videos | Audio
}
```

---

## 요청마다 해석되는 키 (`ApiKeyResolver`) — OpenAI · Anthropic · Google AI · OpenAI Compatible · GPUStack

키를 OS 자격증명 저장소 같은 곳에 두고 거기서 교체·회수한다면, 키 값을 config 에 복사해 넣는 대신
「어디서 가져오는지」를 넘긴다. 리졸버는 **요청마다** 호출되고, 그 값이 provider 의 자격증명 헤더
(`Authorization: Bearer …` · `x-api-key` · `x-goog-api-key`)로 나간다 — 교체한 키가 provider 를 다시 만들지 않아도 다음
호출부터 쓰인다. null/빈 값이면 `ApiKey` 로 돌아간다.

```csharp
builder.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKeyResolver = () => secretStore.Read("openai"),   // 매 요청
});
```

- 소비자가 준 `HttpClient`(Google 은 `HttpClientFactory`)와 함께 쓸 수 없다 — 키는 IronHive 가 만드는 HTTP 클라이언트의
  핸들러(`IronHive.Abstractions.Http.ResolvedCredentialHandler`)가 쓰므로, 그 조합은 생성 시 `InvalidOperationException`
  이다(조용히 정적 키를 영원히 보내는 대신). 자체 `HttpClient` 가 필요하면 그 핸들러를 거기에 직접 넣는다.
- `Validate()` 는 리졸버를 자격증명으로 센다.

## 추가 요청 헤더 (`Headers`) — 네 provider 공통

게이트웨이 뒤의 엔드포인트(API 관리 계층·사내 프록시·테넌트 라우팅)는 자기 헤더를 요구한다 — 구독 키, 테넌트
id, 추적 헤더. 네 provider config(`OpenAIConfig`·`OpenAICompatibleConfig`·`AnthropicConfig`·`GoogleAIConfig`/
`VertexAIConfig`)가 같은 이름·같은 타입의 `Headers`(`IDictionary<string, string>?`)를 갖고, 그 provider가 보내는
**모든 요청**에 실린다. 소비자는 어느 벤더 SDK가 뒤에 있는지 보지 않고 설정한다.

```csharp
new AnthropicConfig
{
    ApiKey = "sk-ant-...",
    Headers = new Dictionary<string, string> { ["Ocp-Apim-Subscription-Key"] = "…", ["X-Tenant-Id"] = "acme" }
}
```

**우선순위 규칙은 문서가 아니라 코드가 강제한다** (`IronHive.Abstractions.Http.ProviderRequestHeaders`):

- **자격증명은 헤더가 아니다.** `Headers`에 자격증명 헤더 이름(`Authorization`, `x-api-key`, `x-goog-api-key` —
  provider별, 대소문자 무관)이 오면 클라이언트 생성 시 `ArgumentException`이 그 이름과 자격증명 슬롯
  (`ApiKey`/`AuthToken`/`Credential`)을 댄다. bearer 토큰 자체를 바꾸는 게이트웨이는 그 키를 `ApiKey`에 준다.
  「설정한 헤더가 자격증명을 덮는가, 덮이는가」가 SDK 파이프라인 위치에 따라 달라지던 상태를 애매한 경우를
  거부해서 없앴다.
- **같은 헤더의 두 출처는 값이 같아야 한다.** Anthropic의 `ExtraHeaders`, Google의 `HttpOptions.Headers`는
  벤더 이름의 같은 슬롯이고 `Headers`와 합집합으로 보내진다. 같은 이름·다른 값은 생성 시 예외.
- 그 외의 헤더는 준 그대로 보내진다. 같은 이름의 SDK 기본값과 만나면 **provider마다 벤더 semantics가 다르다**:
  OpenAI/Compatible은 설정값이 SDK 기본값을 **대체**한다(OpenAI SDK 경로는 `BeforeTransport` policy — `PerCall`에
  두면 credential policy가 덮는 함정이 있어 라이브러리가 그 위치를 소유한다); Anthropic은 벤더 SDK가 `ExtraHeaders`를
  자기 헤더 **뒤에 추가**(`TryAddWithoutValidation`)하므로 같은 이름이면 값이 둘이 간다; Google은 벤더
  `HttpOptions.Headers` semantics를 따른다. 게이트웨이 헤더는 SDK가 스스로 두는 이름이 아니므로 실무에서는
  차이가 없고, `User-Agent` 같은 SDK 이름을 덮으려 할 때만 드러난다.
- 소비자가 준 `HttpClient`의 `DefaultRequestHeaders`는 건드리지 않는다 — 헤더는 요청 단위로 실린다(공유
  `IHttpClientFactory` 클라이언트가 다른 provider와 섞이지 않게).

## 공급자 고유 필드 (`ExtraBody`) — OpenAI Compatible

타입 멤버가 모델링하지 않는 서버 확장 필드를 양방향으로 통과시킨다.

- **요청**: `MessageRequest.ExtraBody`(또는 `AgentInvokeOptions.ExtraBody`)의 필드가 요청 JSON 본문에 deep merge 된다 —
  객체는 객체에 합쳐지고, 그 밖의 값은 그 자리의 값(이 라이브러리가 넣은 필드 포함)을 대체한다. llama.cpp·vLLM 의
  샘플링 확장 같은 값을 보낼 때 쓴다.
- **응답**: 타입 멤버가 매핑하지 않은 **최상위** 응답 필드가 `MessageResponse.ExtraBody`(스트리밍은 done 프레임)에 실린다 —
  llama.cpp 의 `timings`(`prompt_ms`·`predicted_ms`)가 대표적이다. `choices` 안의 필드와 봉투 필드(`object`·`created`)는
  싣지 않는다. 도구 루프에서는 마지막 생성 호출의 값이다. `IChatClient` 브리지는 이것을 `ChatResponse.AdditionalProperties`
  (필드마다 `JsonElement`)로 옮긴다.
- **지원 범위**: OpenAI Compatible(Chat Completions) provider 가 양방향을 지킨다. 공식 SDK 가 요청 본문을 만드는 OpenAI ·
  Anthropic · Google AI provider 는 요청 `ExtraBody` 에 항목이 있으면 조용히 버리지 않고 `NotSupportedException` 을 던지며,
  응답 `ExtraBody` 는 `null` 이다.

```csharp
var response = await messageService.GenerateMessageAsync(new MessageRequest
{
    Provider = "local",
    Model = "qwen3",
    Messages = [Message.User("Hi")],
    ExtraBody = new JsonObject { ["n_probs"] = 5 },
});
var predictedMs = response.ExtraBody?["timings"]?["predicted_ms"]?.GetValue<double>();
```

## 토큰 로그 확률 (`LogProbabilities`) — OpenAI Compatible

`MessageRequest.LogProbabilities = new LogProbabilityOptions { TopAlternatives = k }` 이면 출력 토큰마다 로그 확률과
위치별 상위 `k` 개(0~20) 대안을 요청한다(`logprobs`/`top_logprobs`). `MessageResponse.LogProbabilities` 가 순서대로 싣는다 —
스트리밍은 텍스트 델타마다 그 토큰들을, done 프레임이 전체를 싣는다. 요청하지 않으면 `null`, 요청했는데 서버가 구현하지
않아 싣지 않으면 빈 목록이다. 1 토큰 판정기(`MaxTokens = 1`, `Temperature = 0`)는 `LogProbabilities[0].Alternatives` 로
분포를 읽는다. OpenAI · Anthropic · Google AI provider 는 요청되면 `NotSupportedException` 을 던진다(답을 로그 확률 없이
돌려주지 않는다).

## OpenAI Compatible (범용 호환)

**패키지**: `IronHive.Providers.OpenAI.Compatible`

OpenAI `/v1` API와 호환되는 모든 서버를 지원합니다: Ollama, LM Studio, vLLM, llama.cpp server 등. 이 패키지가 소유한 `ChatCompletionMessageGenerator`가 Chat Completions API(`POST /v1/chat/completions`)를 구현한다. 연결 정보(`BaseUrl`/`ApiKey`/`HttpClient`)만 `IronHive.Providers.OpenAI`의 `OpenAIConfig`를 재사용하고, GPUStack 프로바이더(아래)도 동일한 생성기에 위임한다.

### 등록

`ApiKey`는 **옵션**이다. 이 절이 다루는 서버들은 기본적으로 자격증명을 요구하지 않으므로 생략해도
등록·요청이 모두 동작한다. 요구하는 서버에만 설정한다.

```csharp
// Ollama
builder.AddOpenAICompatibleProviders("ollama", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:11434"
});

// LM Studio
builder.AddOpenAICompatibleProviders("lmstudio", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:1234"
});

// vLLM
builder.AddOpenAICompatibleProviders("vllm", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:8000",
    ApiKey = "..."  // vLLM 인증 토큰 (옵션)
});
```

### ServiceType Flags

```csharp
[Flags]
public enum OpenAICompatibleServiceType
{
    Models = 1,
    Language = 2,
    Embeddings = 4,
    Rerank = 8,
    Images = 16,
    Audio = 32,
    All = Models | Language | Embeddings | Rerank | Images | Audio
}
```

`Images`/`Audio`는 각각 `OpenAIImageGenerator`/`OpenAIAudioProcessor`(둘 다 `IronHive.Providers.OpenAI`)를 `config.ToOpenAI()`로 등록한다 — 실제 지원 여부는 서버/배포된 모델에 따라 다르다(예: 이미지 생성을 지원하지 않는 순수 LLM 서버는 `Images` 요청 시 오류를 반환한다).

### 리랭킹

Cohere 형태의 `POST /rerank`(<https://docs.cohere.com/reference/rerank>)를 구현하는 서버(Infinity, vLLM 등)를 지원한다. `CohereDocumentReranker`가 `IDocumentReranker`를 구현하며, `hive.Rerank`(`IRerankService`)를 통해 프로바이더 이름으로 라우팅된다.

⚠️ 모든 자체 호스팅 rerank 서버가 이 형태를 따르는 건 아니다 — 예를 들어 HuggingFace TEI(text-embeddings-inference)의 `/rerank`는 `documents`/`model`/`top_n` 대신 `texts`/`raw_scores`/`return_text`를 쓰는 독자 포맷이라 이 클라이언트와 호환되지 않는다.

```csharp
builder.AddOpenAICompatibleProviders("tei", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:8080"
}, OpenAICompatibleServiceType.Rerank);

var results = await hive.Rerank.RerankAsync(
    "tei", "rerank-model", "가장 관련 있는 문서는?",
    documents: ["문서 A", "문서 B", "문서 C"],
    topN: 2);
// results: documents의 원본 인덱스 + 점수, 점수 내림차순
```

---

## GPUStack

**패키지**: `IronHive.Providers.OpenAI.Compatible`

GPUStack 전용 최적화 프로바이더 (base URL 경로 `/v1-openai/`). 메시지 생성은 별도 클래스 없이 `OpenAICompatibleMessageGenerator`가 처리한다 — `GpuStackConfig.ToOpenAICompatible()`(internal)이 `OpenAICompatibleConfig`로 변환하고(`Path`를 `/v1-openai/`로 고정, resolver·`TokenLimitParameter`·`ConnectTimeout` 그대로 전달), 그 결과를 넘긴다. `OpenAICompatibleConfig`용 제네레이터와 로직이 완전히 동일했기 때문에 GPUStack 전용 클래스는 제거되었다. `GpuStackConfig.ToOpenAI()`도 동일하게 표면을 `ChatCompletions`로 고정한다.

```csharp
builder.AddGpuStackProviders("gpustack", new GpuStackConfig
{
    BaseUrl = "http://gpustack-server:8080",
    ApiKey = "..."
});
```

### ServiceType Flags

```csharp
[Flags]
public enum GpuStackServiceType
{
    Models = 1,
    Language = 2,
    Embeddings = 4,
    Rerank = 8,
    Images = 16,
    Audio = 32,
    All = Models | Language | Embeddings | Rerank | Images | Audio
}
```

`Images`(SGLang 백엔드)/`Audio`(VoxBox 백엔드, TTS/STT)도 chat/embeddings/models와 같은 `/v1-openai/` 경로를 쓰므로 `config.ToOpenAI()`를 그대로 재사용한다.

### 리랭킹

GPUStack은 Jina 호환(Cohere 호환의 상위 호환) `POST /v1/rerank`를 제공한다 — llama-box 백엔드에서만 지원되며, chat/embeddings/models가 쓰는 `/v1-openai/` 경로와 다르다. `GpuStackConfig.ToRerankConfig()`(internal)가 이 경로를 targeting하는 `OpenAIConfig`를 만들고, `CohereDocumentReranker`(`IronHive.Providers.OpenAI.Compatible.Reranking`)에 넘겨진다.

```csharp
builder.AddGpuStackProviders("gpustack", new GpuStackConfig
{
    BaseUrl = "http://gpustack-server:8080",
    ApiKey = "..."
}, GpuStackServiceType.Rerank);

var results = await hive.Rerank.RerankAsync(
    "gpustack", "bge-reranker-v2-m3", "query", documents: ["doc1", "doc2"]);
```

---

## 공통 메시지 요청 파라미터

에이전트의 `AgentParametersConfig` 또는 `MessageRequest`에서 설정:

```csharp
var request = new MessageRequest
{
    Provider = "openai",
    Model = "gpt-4o-mini",
    Messages = messages,
    // 생성 파라미터
    ThinkingEffort = MessageThinkingEffort.High,  // 추론 노력도 (지원 모델)
    ThinkingOutput = MessageThinkingOutput.Summary, // 추론 내용 노출 (None · Summary · Full, 미설정은 provider 기본)
    // AgentConfig.Parameters에서 설정
};

// AgentParametersConfig
config.Parameters = new AgentParametersConfig
{
    MaxTokens = 4096,
    Temperature = 0.7f,
    TopP = 0.9f,
    TopK = 50,
    StopSequences = ["END", "STOP"]
};
```

`ThinkingOutput` 은 추론을 **보여 줄지만** 정한다 — 추론을 할지·얼마나 할지는 `ThinkingEffort` 다(`ChatOptions.Reasoning.Output`
/ `.Effort` 와 같은 분리). 미설정이면 provider 기본을 그대로 둔다.

| provider | `None` | `Summary` | `Full` |
|---|---|---|---|
| Google AI | `includeThoughts: false` | `includeThoughts: true`(노력도 없이도 — 기본으로 생각하는 모델의 요약) | `Summary` 와 같음 |
| Anthropic (Adaptive 세대, 노력도와 함께) | `display: omitted` — thinking 블록은 빈 텍스트 + 서명으로 남는다 | `display: summarized` | `summarized`(원문 사고는 제공되지 않음) |
| OpenAI (Responses) | 요약 미요청(`reasoning.encrypted_content` 는 멀티턴 연속성용으로 유지) | `summary: auto` | `summary: detailed` |

Anthropic Budget 세대(Claude 4.x)와 Gemini 1.5/2.0 에는 노출 파라미터가 없어 무시된다. 2026-09-17 live 실측:
gemini-2.5-flash Summary → 요약 861자 · None/미설정 → 0, claude-sonnet-5 None → 빈 thinking 블록 · Summary → 요약 텍스트.

---

## 프로바이더 선택 가이드

| 요구사항 | 추천 |
|----------|------|
| 최고 성능 | OpenAI (GPT-4o), Anthropic (Claude 3.5+) |
| 비용 효율 | OpenAI (GPT-4o-mini) |
| 프라이버시 / 온프레미스 | Ollama, LM Studio, GPUStack |
| 멀티모달 (이미지/비디오) | Google AI (Gemini Pro), OpenAI (GPT-4V) |
| 긴 컨텍스트 | Anthropic (200K), Google AI (1M) |
| 확장 사고 (Thinking) | Anthropic (Extended Thinking), Google AI (Gemini 2.0 Thinking) |
| 임베딩 | OpenAI (text-embedding-3), Google AI (text-embedding-004) |
| 이미지 생성 | OpenAI (DALL-E 3), Google AI (Imagen 3) |

---

## 관련 문서

- [SETUP.md](SETUP.md) — 설정 및 DI 통합
- [ARCHITECTURE.md](ARCHITECTURE.md) — 아키텍처 개요
