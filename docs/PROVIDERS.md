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
| `OpenAIMessageGenerator` | 메시지 생성 파사드. `OpenAIConfig.Api`가 선택한 표면으로 위임 |
| `OpenAIResponseMessageGenerator` | Responses API(`/v1/responses`) 구현. reasoning summary/encrypted content |
| `OpenAIChatMessageGenerator` | Chat Completions API(`/v1/chat/completions`) 구현 |
| `OpenAIEmbeddingGenerator` | 텍스트 임베딩 |
| `OpenAIImageGenerator` | DALL-E 이미지 생성/편집 |
| `OpenAIAudioProcessor` | TTS (tts-1, tts-1-hd) + STT (Whisper) |
| `OpenAIModelFinder` | 사용 가능한 모델 목록 조회 |

### API 표면 선택 (`OpenAIConfig.Api`)

OpenAI는 채팅에 대해 **두 가지 wire-비호환 HTTP 표면**을 제공한다. `OpenAIMessageGenerator`는 `OpenAIConfig.Api`(`OpenAIApiSurface`)로 어느 쪽을 타격할지 선택한다.

| 값 | 엔드포인트 | 용도 |
|----|-----------|------|
| `Responses` **(first-party 기본)** | `POST /v1/responses` | OpenAI 자체 표면. reasoning summary, `reasoning.encrypted_content`, `xhigh` effort 지원 |
| `ChatCompletions` | `POST /v1/chat/completions` | 사실상의 표준. OpenAI-호환/셀프호스트 서버(Ollama, LM Studio, vLLM, llama.cpp, GPUStack)가 구현하는 표면 |

> ⚠️ Chat-Completions-only 엔드포인트를 `Responses` 표면으로 가리키면(또는 그 반대) **컴파일/restore는 통과하고 런타임에 `404 Not Found`로만** 드러난다. 셀프호스트 엔드포인트는 아래 `IronHive.Providers.OpenAI.Compatible`를 쓰면 표면이 자동으로 `ChatCompletions`로 설정된다.

### 등록

```csharp
// 모든 서비스 한 번에 등록 — first-party OpenAI, 기본 Responses 표면
builder.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKey = "sk-..."
});

// Chat Completions 표면이 필요하면 명시적으로 선택
builder.AddOpenAIProviders("openai-chat", new OpenAIConfig
{
    ApiKey = "sk-...",
    Api = OpenAIApiSurface.ChatCompletions
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
    public OpenAIApiSurface Api { get; set; }  // Responses(기본) 또는 ChatCompletions
    // + MaxRetries, Timeout, HttpClient 등
}
```

### 지원 기능

- Chat Completions API + Responses API
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

> API 표면별 상세: 채팅 완성(Chat Completions)/Responses 양쪽 다 도구 호출·스트리밍을 지원한다. 추론 노력도(Reasoning Effort)는 **`Responses` 표면 전용** — Chat Completions 표면은 reasoning 입력/effort를 다루지 않는다(호환 서버 대다수가 `reasoning_effort` 미지원).

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
    ApiKey = "AIza..."
});

// Vertex AI
builder.AddVertexAIProviders("vertex", new VertexAIConfig
{
    ProjectId = "my-project",
    Location = "us-central1"
    // 자격증명은 Application Default Credentials 사용
});
```

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

OpenAI `/v1` API와 호환되는 모든 서버를 지원합니다: Ollama, LM Studio, vLLM, llama.cpp server 등. 내부적으로 `IronHive.Providers.OpenAI`의 Chat Completions 구현(`OpenAIChatMessageGenerator`)을 재사용하며, `OpenAICompatibleConfig.ToOpenAI()`가 표면을 자동으로 `OpenAIApiSurface.ChatCompletions`로 고정한다 — 이들 서버는 OpenAI 자체 Responses API를 구현하지 않으므로 Chat Completions가 유일하게 올바른 표면이다.

### 등록

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
