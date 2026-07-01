# 설정 가이드

IronHive 초기 설정 및 DI 통합 방법입니다.

## 설치

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI    # 또는 Anthropic, GoogleAI 등
```

---

## HiveServiceBuilder

`HiveServiceBuilder`로 프로바이더와 스토리지를 등록한 후 `IHiveService`를 빌드합니다.

### 기본 패턴

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

var hive = new HiveServiceBuilder()
    // 프로바이더 등록 (이름으로 식별)
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "..." })
    .AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = "..." })

    // 스토리지 등록
    .AddLocalVectorStorage("local-vec", new LocalVectorConfig { Path = "./data/vectors.db" })
    .AddLocalQueueStorage("local-queue", new LocalQueueConfig { Path = "./data/queue" })
    .AddLocalFileStorage("local-files")

    .Build();
```

### IHiveServiceBuilder 메서드

| 메서드 | 설명 |
|--------|------|
| `AddModelFinder(name, finder)` | 모델 목록 조회기 등록 |
| `AddMessageGenerator(name, gen)` | LLM 텍스트 생성기 등록 |
| `AddEmbeddingGenerator(name, gen)` | 임베딩 생성기 등록 |
| `AddImageGenerator(name, gen)` | 이미지 생성기 등록 |
| `AddVideoGenerator(name, gen)` | 비디오 생성기 등록 |
| `AddAudioProcessor(name, proc)` | 오디오 처리기 등록 |
| `AddFileStorage(name, storage)` | 파일 스토리지 등록 |
| `AddVectorStorage(name, storage)` | 벡터 스토리지 등록 |
| `AddQueueStorage(name, storage)` | 큐 스토리지 등록 |
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
    ApiKey = "AIza..."
}, GoogleAIServiceType.All);

// Vertex AI
builder.AddVertexAIProviders("vertex", new VertexAIConfig { ... });

// OpenAI Compatible (Ollama, LM Studio, vLLM, llama.cpp 등)
builder.AddOpenAICompatibleProviders("ollama", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:11434"
});

// GPUStack
builder.AddGpuStackProviders("gpustack", new GpuStackConfig { ... });
```

### ServiceType [Flags] 옵션

```csharp
// 일부만 등록
builder.AddOpenAIProviders("openai", config,
    OpenAIServiceType.Messages | OpenAIServiceType.Embeddings);
```

---

## 로컬 스토리지 편의 메서드

```csharp
// 로컬 파일 스토리지 (디렉토리 기반)
builder.AddLocalFileStorage("local");

// 로컬 벡터 스토리지 (SQLite + sqlite-vec)
builder.AddLocalVectorStorage("local-vec", new LocalVectorConfig
{
    Path = "./vectors.db"
});

// 로컬 큐 스토리지 (파일 시스템 기반)
builder.AddLocalQueueStorage("local-queue", new LocalQueueConfig
{
    Path = "./queue-data"
});
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
        .AddLocalVectorStorage("local", new LocalVectorConfig { Path = "./vectors.db" })
        .Build());

// 파일 파서 등록 (PDF, Word, Excel, PPT, Image)
builder.Services.AddFileParser();
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
var hive = new HiveServiceBuilder()...Build();
builder.Services.AddHiveService(hive);
```

---

## AddHiveService 오버로드

```csharp
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

## 파일 파서 등록

`AddFileParser()`는 다음 파서를 DI에 등록합니다:

| 파서 | 지원 형식 |
|------|----------|
| `PdfParser` | PDF |
| `WordParser` | DOCX |
| `ExcelParser` | XLSX |
| `PowerPointParser` | PPTX |
| `ImageParser` | PNG, JPG, GIF 등 |

미지원 형식은 null byte 휴리스틱으로 텍스트/바이너리 자동 판별.

```csharp
services.AddFileParser();

// DI에서 사용
public class DocumentService(IFileParserService parserService)
{
    public async Task<string> ParseAsync(Stream stream, string contentType)
    {
        var blocks = await parserService.ParseAsync(stream, contentType);
        return string.Concat(blocks.Select(b => b.Text));
    }
}
```

---

## 관련 문서

- [PROVIDERS.md](PROVIDERS.md) — 프로바이더 상세 설정
- [STORAGES.md](STORAGES.md) — 스토리지 백엔드
- [ARCHITECTURE.md](ARCHITECTURE.md) — 전체 아키텍처
