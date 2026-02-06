# IronHive.Providers.OpenAI.Compatible

OpenAI API와 호환되는 다양한 서비스 제공자를 지원하는 패키지입니다.
각 Provider의 상세 특이사항은 해당 폴더의 README.md를 참고하세요.

---

## 지원 Provider 목록

| Provider | Base URL | API 타입 | 상세 문서 |
|----------|----------|----------|-----------|
| xAI (Grok) | `https://api.x.ai/v1` | Responses API | [XAI/README.md](XAI/README.md) |
| Groq | `https://api.groq.com/openai/v1` | Chat Completions API | [Groq/README.md](Groq/README.md) |
| DeepSeek | `https://api.deepseek.com/v1` | Chat Completions API | [DeepSeek/README.md](DeepSeek/README.md) |
| Together AI | `https://api.together.xyz/v1` | Chat Completions API | [TogetherAI/README.md](TogetherAI/README.md) |
| Fireworks AI | `https://api.fireworks.ai/inference/v1` | Chat Completions API | [Fireworks/README.md](Fireworks/README.md) |
| Perplexity | `https://api.perplexity.ai` | Chat Completions API | [Perplexity/README.md](Perplexity/README.md) |
| OpenRouter | `https://openrouter.ai/api/v1` | Chat Completions API | [OpenRouter/README.md](OpenRouter/README.md) |
| vLLM | 사용자 정의 (기본: `http://localhost:8000/v1`) | Chat/Responses API | Self-hosted |
| GPUStack | 사용자 정의 | Chat Completions API | Self-hosted |

> **API 타입 컬럼**: 현재 IronHive에서 해당 Provider의 기본 MessageGenerator가 사용하는 API 방식입니다.

---

## 아키텍처

### 핵심 클래스

#### `CompatibleConfig` (추상 클래스)

모든 Provider Config의 기반 클래스입니다. 각 Provider는 이를 상속하여 자체 설정을 정의합니다.

```csharp
public abstract class CompatibleConfig
{
    public string? ApiKey { get; set; }
    public abstract OpenAIConfig ToOpenAI();
}
```

- `ApiKey`: 공통 인증 속성
- `ToOpenAI()`: 각 Provider가 자신의 Base URL, 헤더 등을 `OpenAIConfig`로 변환하는 메서드

#### `CompatibleChatMessageGenerator`

Chat Completions API를 사용하는 Provider의 기본 MessageGenerator입니다.
`OpenAIChatMessageGenerator`를 상속하며, `CompatibleConfig.ToOpenAI()`를 통해 OpenAI 클라이언트를 생성합니다.

```csharp
internal class CompatibleChatMessageGenerator : OpenAIChatMessageGenerator
{
    protected readonly CompatibleConfig _config;

    public CompatibleChatMessageGenerator(CompatibleConfig config)
        : base(config.ToOpenAI()) { }
}
```

#### `CompatibleResponseMessageGenerator`

Responses API를 사용하는 Provider의 기본 MessageGenerator입니다.
`OpenAIResponseMessageGenerator`를 상속합니다.

```csharp
internal class CompatibleResponseMessageGenerator : OpenAIResponseMessageGenerator
{
    protected readonly CompatibleConfig _config;

    public CompatibleResponseMessageGenerator(CompatibleConfig config)
        : base(config.ToOpenAI()) { }
}
```

### 클래스 관계도

```
CompatibleConfig (abstract)
├── XAIConfig
├── GroqConfig
├── DeepSeekConfig
├── TogetherAIConfig
├── FireworksConfig
├── PerplexityConfig
└── OpenRouterConfig

OpenAIChatMessageGenerator
└── CompatibleChatMessageGenerator
    ├── GroqMessageGenerator
    ├── DeepSeekMessageGenerator
    ├── TogetherAIMessageGenerator
    ├── FireworksMessageGenerator
    ├── PerplexityMessageGenerator
    └── OpenRouterMessageGenerator

OpenAIResponseMessageGenerator
└── CompatibleResponseMessageGenerator
    └── XAIMessageGenerator
```

---

## 새 Provider 추가 가이드

새로운 OpenAI 호환 Provider를 추가할 때 다음 단계를 따릅니다.

### 1. 폴더 생성

프로젝트 루트에 Provider 이름으로 폴더를 생성합니다.

```
{ProviderName}/
├── {ProviderName}Config.cs
├── {ProviderName}MessageGenerator.cs
└── README.md
```

### 2. Config 클래스 작성

`CompatibleConfig`를 상속하여 Provider별 설정 클래스를 작성합니다.

```csharp
public class NewProviderConfig : CompatibleConfig
{
    private const string DefaultBaseUrl = "https://api.newprovider.com/v1";

    // Provider 고유 설정 속성들...

    public override OpenAIConfig ToOpenAI()
    {
        return new OpenAIConfig
        {
            BaseUrl = DefaultBaseUrl,
            ApiKey = ApiKey ?? string.Empty,
            // DefaultHeaders = ... (필요 시)
        };
    }
}
```

**주의사항:**
- Base URL은 `private const string`으로 정의
- `ToOpenAI()`에서 Provider 고유의 헤더가 필요하면 `DefaultHeaders`에 추가 (OpenRouter 참고)

### 3. MessageGenerator 클래스 작성

Provider가 사용하는 API 타입에 따라 기반 클래스를 선택합니다:

| API 타입 | 기반 클래스 |
|----------|------------|
| Chat Completions API | `CompatibleChatMessageGenerator` |
| Responses API | `CompatibleResponseMessageGenerator` |

```csharp
internal class NewProviderMessageGenerator : CompatibleChatMessageGenerator
{
    private readonly NewProviderConfig _providerConfig;

    public NewProviderMessageGenerator(NewProviderConfig config) : base(config)
    {
        _providerConfig = config;
    }

    // Provider 고유 처리가 필요한 경우 PostProcessRequest를 override
    // protected override T PostProcessRequest<T>(ChatCompletionRequest request)
    // {
    //     // Provider별 파라미터 주입, 미지원 파라미터 제거 등
    //     return (T)request;
    // }
}
```

### 4. Extension Method 등록

`Extensions/HiveServiceBuilderExtensions.cs`에 Provider 등록 메서드를 추가합니다.

```csharp
public static IHiveServiceBuilder AddNewProvider(
    this IHiveServiceBuilder builder,
    string providerName,
    NewProviderConfig config)
{
    builder.AddMessageGenerator(providerName, new NewProviderMessageGenerator(config));
    return builder;
}
```

### 5. README.md 작성

각 Provider 폴더에 README.md를 작성합니다. 포함해야 할 내용:

- 공식 문서 링크 및 Base URL
- 호환성 (지원하는 API 목록)
- 주요 모델
- 특수 파라미터 및 기능
- 제한사항
- IronHive 구현 상태 (Config 속성, MessageGenerator 정보)
- TODO (미구현 사항)
- Last Updated 날짜

---

## Self-hosted Provider 사용

vLLM, GPUStack 등 Self-hosted 서비스는 별도 Config 없이 `AddCompatibleProvider`를 사용합니다.

```csharp
// vLLM
builder.AddCompatibleProvider("vllm", new VLLMConfig
{
    ApiKey = "optional-api-key",
});

// GPUStack
builder.AddCompatibleProvider("gpustack", new GPUStackConfig
{
    ApiKey = "optional-api-key",
});
```

Self-hosted Provider는 `CompatibleConfig`를 직접 구현하며, `ToOpenAI()`에서 사용자 지정 Base URL을 설정합니다.
`AddCompatibleProvider`는 내부적으로 `OpenAIConfig`로 변환하여 OpenAI Provider의 전체 기능(Chat, Responses, Embeddings 등)을 그대로 활용합니다.

---

## 참고 자료

- [xAI API Reference](https://docs.x.ai/docs/api-reference)
- [Groq API Reference](https://console.groq.com/docs/api-reference)
- [DeepSeek API Docs](https://api-docs.deepseek.com/)
- [OpenRouter Documentation](https://openrouter.ai/docs)
- [Together AI Docs](https://docs.together.ai/)
- [Fireworks AI Docs](https://docs.fireworks.ai/)
- [Perplexity API](https://docs.perplexity.ai/)
- [vLLM Documentation](https://docs.vllm.ai/)
- [AI SDK OpenAI Compatible Providers](https://ai-sdk.dev/providers/openai-compatible-providers)
- [LiteLLM Providers](https://docs.litellm.ai/docs/providers)

---

*Last Updated: 2026-02-06*
