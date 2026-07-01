# IronHive

<p align="center">
  <img src="assets/ironhive.png" alt="IronHive Logo" width="200"/>
</p>

<p align="center">
  <a href="https://github.com/iyulab/ironhive/actions/workflows/ci.yml">
    <img src="https://github.com/iyulab/ironhive/actions/workflows/ci.yml/badge.svg" alt="CI">
  </a>
  <a href="https://www.nuget.org/packages/IronHive.Core">
    <img src="https://img.shields.io/nuget/v/IronHive.Core?label=NuGet" alt="NuGet">
  </a>
  <a href="https://github.com/iyulab/ironhive/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/iyulab/ironhive" alt="License">
  </a>
</p>

**IronHive**는 기업용 AI 애플리케이션을 위한 .NET 10 파이프라인 프레임워크입니다. 이름 기반 레지스트리 패턴으로 멀티 Provider LLM 통합, 멀티에이전트 오케스트레이션, RAG 파이프라인, 파일 처리를 제공합니다.

## 주요 기능

- **멀티 Provider LLM** — OpenAI, Anthropic, Google AI (Gemini/Vertex AI), OpenAI Compatible (Ollama, LM Studio, GPUStack 등)
- **멀티에이전트 오케스트레이션** — Sequential, Parallel, Handoff, GroupChat, HubSpoke, Graph (DAG)
- **RAG 파이프라인** — 텍스트 추출, 청킹, 임베딩, 벡터 검색
- **다중 모달리티** — 이미지 생성, 음성 TTS/STT, 비디오 생성
- **플러그인** — MCP (HTTP/Stdio/OAuth), OpenAPI 자동 도구 생성
- **M.E.AI 호환** — `ChatClientAdapter` / `EmbeddingGeneratorAdapter`
- **워크플로우** — 코드 기반 타입 안전 워크플로우 엔진

## 설치

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI    # 또는 Anthropic, GoogleAI 등
```

## 빠른 시작

### Standalone (콘솔)

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;
using IronHive.Abstractions.Messages.Content;

var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "your-api-key" })
    .Build();

var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider = "openai";
    cfg.Model = "gpt-4o-mini";
    cfg.Instructions = "당신은 친절한 도우미입니다.";
});

// 단순 텍스트 호출
var response = await agent.InvokeAsync("안녕하세요");

// 스트리밍
await foreach (var chunk in agent.InvokeStreamingAsync("안녕하세요"))
{
    // chunk 처리
}
```

### ASP.NET Core DI 통합

```csharp
// Program.cs
builder.Services.AddHiveService((hiveBuilder, sp) =>
    hiveBuilder
        .AddOpenAIProviders("openai", new OpenAIConfig
        {
            ApiKey = builder.Configuration["OpenAI:ApiKey"]!
        })
        .Build());

// 서비스에서 IHiveService 주입
public class ChatService(IHiveService hive)
{
    public async Task<string> ChatAsync(string text)
    {
        var agent = hive.CreateAgentFrom(cfg =>
        {
            cfg.Provider = "openai";
            cfg.Model = "gpt-4o-mini";
        });
        var response = await agent.InvokeAsync(text);
        return response.Message?.Content
            .OfType<TextMessageContent>()
            .FirstOrDefault()?.Value ?? string.Empty;
    }
}
```

## 패키지

| 패키지 | 설명 |
|--------|------|
| `IronHive.Abstractions` | 인터페이스 및 계약 (외부 의존 없음) |
| `IronHive.Core` | 핵심 구현 (에이전트, 오케스트레이터, 워크플로우) |
| `IronHive.Providers.OpenAI` | OpenAI (Chat, Embeddings, DALL-E, TTS/STT) |
| `IronHive.Providers.Anthropic` | Anthropic Claude |
| `IronHive.Providers.GoogleAI` | Google Gemini + Vertex AI (이미지, 비디오, 오디오 포함) |
| `IronHive.Providers.OpenAI.Compatible` | Ollama, LM Studio, vLLM, GPUStack 등 |
| `IronHive.Storages.Qdrant` | Qdrant 벡터 데이터베이스 |
| `IronHive.Storages.Amazon` | Amazon S3 파일 저장소 |
| `IronHive.Storages.Azure` | Azure Blob / File Share |
| `IronHive.Storages.RabbitMQ` | RabbitMQ 큐 |
| `IronHive.Plugins.MCP` | Model Context Protocol (HTTP/Stdio/OAuth) |
| `IronHive.Plugins.OpenAPI` | OpenAPI 도구 자동 생성 |

## 문서

| 문서 | 설명 |
|------|------|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | 시스템 아키텍처 및 설계 원칙 |
| [docs/SETUP.md](docs/SETUP.md) | HiveServiceBuilder 구성 및 DI 통합 |
| [docs/AGENTS.md](docs/AGENTS.md) | 에이전트 생성 및 호출 |
| [docs/MIDDLEWARE.md](docs/MIDDLEWARE.md) | 미들웨어 시스템 (Retry, Timeout, CircuitBreaker 등) |
| [docs/ORCHESTRATION.md](docs/ORCHESTRATION.md) | 멀티에이전트 오케스트레이션 패턴 |
| [docs/TOOLS.md](docs/TOOLS.md) | FunctionTool 및 커스텀 도구 |
| [docs/MEMORY.md](docs/MEMORY.md) | RAG 파이프라인 및 MemoryWorker |
| [docs/PROVIDERS.md](docs/PROVIDERS.md) | AI 프로바이더 설정 |
| [docs/STORAGES.md](docs/STORAGES.md) | 스토리지 백엔드 설정 |
| [docs/PLUGINS.md](docs/PLUGINS.md) | MCP / OpenAPI 플러그인 |
| [docs/SERVICES.md](docs/SERVICES.md) | IHiveService 서비스 상세 |

## Skills (AI 코딩 에이전트용)

AI 코딩 에이전트(GitHub Copilot, Claude Code, Cursor 등)에서 IronHive Skills를 사용하려면:

```bash
npx skills add iyulab/ironhive
```

## 요구 사항

- .NET 10.0+

## 라이선스

MIT — [LICENSE](./LICENSE) 참조.
