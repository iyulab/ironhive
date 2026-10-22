# 설정 가이드

IronHive 초기 설정 및 DI 통합 방법입니다.

## 설치

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI    # 또는 Anthropic, GoogleAI 등
```

---

## HiveServiceBuilder

`HiveServiceBuilder`로 프로바이더를 등록한 후 `IHiveService`를 빌드합니다.

### 기본 패턴

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

var hive = new HiveServiceBuilder()
    // 프로바이더 등록 (이름으로 식별)
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "..." })
    .AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = "..." })
    .Build();
```

### IHiveServiceBuilder 메서드

| 메서드 | 설명 |
|--------|------|
| `AddModelFinder(name, finder)` | 모델 목록 조회기 등록 |
| `AddMessageGenerator(name, gen)` | LLM 텍스트 생성기 등록 |
| `AddEmbeddingGenerator(name, gen)` | 임베딩 생성기 등록 |
| `AddDocumentReranker(name, reranker)` | 리랭커 등록 |
| `AddImageGenerator(name, gen)` | 이미지 생성기 등록 |
| `AddVideoGenerator(name, gen)` | 비디오 생성기 등록 |
| `AddAudioProcessor(name, proc)` | 오디오 처리기 등록 |
| `Build()` | `IHiveService` 인스턴스 생성 |

---

## 프로바이더 등록 편의 메서드

각 프로바이더 패키지는 모든 서비스를 한 번에 등록하는 확장 메서드를 제공합니다:

```csharp
// OpenAI (Messages + Embeddings + Images + Audio + Models)
builder.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKey = "sk-..."
}, OpenAIServiceType.All);

// Anthropic (Messages + Models)
builder.AddAnthropicProviders("anthropic", new AnthropicConfig
{
    ApiKey = "sk-ant-..."
}, AnthropicServiceType.All);

// Google AI (Messages + Embeddings + Images + Videos + Audio + Models)
builder.AddGoogleAIProviders("google", new GoogleAIConfig
{
    ApiKey = "AQ...."   // AI Studio auth key — 표준 키(AIza…)는 2026-09부터 Gemini API가 거부
}, GoogleAIServiceType.All);

// Vertex AI
builder.AddVertexAIProviders("vertex", new VertexAIConfig { Project = "my-project", Location = "us-central1" });

// OpenAI Compatible (Ollama, LM Studio, vLLM, llama.cpp 등)
builder.AddOpenAICompatibleProviders("ollama", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:11434"
});

// GPUStack
builder.AddGpuStackProviders("gpustack", new GpuStackConfig { BaseUrl = "http://localhost:80", ApiKey = "your-key" });
```

### ServiceType [Flags] 옵션

```csharp
// 일부만 등록
builder.AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." },
    OpenAIServiceType.Messages | OpenAIServiceType.Embeddings);
```

---

## ASP.NET Core DI 통합

```csharp
// Program.cs
builder.Services.AddHiveService((hiveBuilder, serviceProvider) =>
    hiveBuilder
        .AddOpenAIProviders("openai", new OpenAIConfig
        {
            ApiKey = builder.Configuration["OpenAI:ApiKey"]!
        })
        .Build());
```

### DI에서 사용

```csharp
public class ChatService(IHiveService hive)
{
    public async Task<string> ChatAsync(string userMessage)
    {
        var agent = hive.CreateAgentFrom(cfg =>
        {
            cfg.Provider = "openai";
            cfg.Model = "gpt-4o-mini";
            cfg.Instructions = "당신은 친절한 도우미입니다.";
        });

        var response = await agent.InvokeAsync(userMessage);
        return response.Message?.Content
            .OfType<TextMessageContent>()
            .FirstOrDefault()?.Value ?? string.Empty;
    }
}
```

### ServiceLifetime 제어

```csharp
// 기본 Scoped (요청마다 새로운 IHiveService)
builder.Services.AddHiveService((b, sp) => b.Build());

// Singleton으로 변경
builder.Services.AddHiveService((b, sp) => b.Build(),
    lifetime: ServiceLifetime.Singleton);

// 기존 인스턴스 등록
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." })
    .Build();
builder.Services.AddHiveService(hive);
```

---

## AddHiveService 오버로드

```csharp
using Microsoft.Extensions.Configuration;

// 팩토리 방식 (IServiceProvider 접근 가능)
services.AddHiveService((builder, sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    return builder
        .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = config["OpenAI:ApiKey"]! })
        .Build();
});

// 기존 인스턴스 직접 등록
services.AddHiveService(existingHiveInstance);
```

---

## 관련 문서

- [PROVIDERS.md](PROVIDERS.md) — 프로바이더 상세 설정
- [ARCHITECTURE.md](ARCHITECTURE.md) — 전체 아키텍처
