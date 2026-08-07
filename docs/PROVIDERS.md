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
    public string ApiKey { get; set; }
    public string? BaseUrl { get; set; }  // Azure OpenAI나 커스텀 엔드포인트용
    // + MaxRetries, Timeout, HttpClient 등
}
```

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
    public string? ApiKey { get; set; }      // API 키 또는 AuthToken 중 하나 필수
    public string? AuthToken { get; set; }
    public string? BaseUrl { get; set; }
    public Dictionary<string, string>? ExtraHeaders { get; set; }
    public int? MaxRetries { get; set; }
    public TimeSpan? Timeout { get; set; }
}
```

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
    Timeout = TimeSpan.FromMinutes(10)   // 생략 시 GoogleAIDefaults.Timeout (10분)
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

`GoogleAIConfig.Timeout` / `VertexAIConfig.Timeout`이 요청 타임아웃을 정한다. 생략하면
`GoogleAIDefaults.Timeout`(10분)이 적용된다 — 벤더 SDK는 타임아웃이 지정되지 않으면 `HttpClient`
기본값 100초를 그대로 쓰는데, 그 값은 비스트리밍 호출에서 응답 전체를, 스트리밍 호출에서 첫 바이트까지의
시간을 제한한다. 어댑터가 명시적 기본값을 두어 그것이 조용히 상속되지 않게 한다.

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
    All = Models | Language | Embeddings
}
```

---

## GPUStack

**패키지**: `IronHive.Providers.OpenAI.Compatible`

GPUStack 전용 최적화 프로바이더 (`GpuStackMessageGenerator`, base URL 경로 `/v1-openai/`). `GpuStackConfig.ToOpenAI()`도 동일하게 표면을 `ChatCompletions`로 고정한다.

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
    All = Models | Language | Embeddings
}
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
