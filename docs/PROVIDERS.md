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
    ApiKey = "AIza...",
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

### 타임아웃

`GoogleAIConfig.Timeout` / `VertexAIConfig.Timeout`이 요청 타임아웃을 정한다. **생략하면 요청
타임아웃을 두지 않는다** — 대신 `ConnectTimeout`(기본 5초)이 TCP 연결 수립을 제한하므로, 응답이
없는 호스트에서 무한정 멈추지는 않는다. 어댑터는 `HttpClientFactory`를 지정하지 않으면 항상 자체
`HttpClient`를 공급해 `Timeout`을 무제한으로 두므로, 벤더 SDK가 만드는 바닐라 `HttpClient`의 100초
기본값을 조용히 물려받는 일이 없다.

`HttpOptions`로도 같은 값을 지정할 수 있으나(밀리초 단위) **둘을 동시에 설정하면
`InvalidOperationException`을 던진다.** 어느 쪽이 이겼는지 알 수 없는 상태를 만들지 않기 위한 것이며,
`HttpOptions`는 `BaseUrl` 등 나머지 설정에 계속 쓸 수 있다.

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
