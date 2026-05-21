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

**IronHive**는 기업용 AI 애플리케이션을 위한 .NET 파이프라인 프레임워크입니다. 멀티 Provider LLM 통합, 벡터 기반 RAG 파이프라인, 멀티에이전트 오케스트레이션, 파일 처리를 Fluent Builder API로 제공합니다.

범용 프레임워크가 필요하다면 [Semantic Kernel](https://github.com/microsoft/semantic-kernel) 또는 [Agent Framework](https://github.com/microsoft/agent-framework)를 권장합니다.

## 주요 기능

- **멀티 Provider LLM** — OpenAI, Anthropic, Google AI, Ollama
- **멀티에이전트 오케스트레이션** — Sequential, Parallel, Hub-Spoke, Graph (DAG)
- **RAG 파이프라인** — 텍스트 추출, 청킹, 임베딩, 벡터 검색
- **파일 처리** — PDF, Word, PowerPoint, 이미지
- **플러그인** — MCP, OpenAPI 통합
- **M.E.AI 호환** — `ChatClientAdapter` / `EmbeddingGeneratorAdapter`
- **워크플로우** — `IHiveService.Workflows`를 통해 YAML/JSON/정의 기반 워크플로우 빌드 (`IWorkflowFactory`)

## 설치

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI    # 또는 Anthropic, GoogleAI, Ollama
```

## 빌더 API 규칙

`HiveServiceBuilder`의 등록 메서드는 두 가지 동사를 구분합니다:

| 동사 | 의미 |
|------|------|
| `AddX(name, item)` | 신규 등록. 동일 이름이 이미 존재하면 `InvalidOperationException` |
| `SetX(name, item)` | Upsert. 동일 이름이 있으면 교체, 없으면 추가 |

```csharp
builder
    .AddMessageGenerator("openai", gen1)   // 첫 등록 — OK
    .SetMessageGenerator("openai", gen2)   // 교체 — OK
    // .AddMessageGenerator("openai", gen3) // 두 번 Add — throws InvalidOperationException
    ;
```

## 빠른 시작

### Standalone (콘솔, 단순 스크립트)

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;
using IronHive.Abstractions.Messages.Content;

var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", new OpenAIMessageGenerator(new OpenAIConfig
    {
        ApiKey = "your-api-key"
    }))
    .Build();

// Provider가 하나면 자동 선택 — Provider 생략 가능
var agent = hive.CreateAgent(config =>
{
    config.Model = "gpt-4o";
    config.Instructions = "You are a helpful assistant.";
});

// string 오버로드로 단순 호출
var response = await agent.InvokeAsync("안녕하세요");
var text = response.Message?.Content
    .OfType<TextMessageContent>().FirstOrDefault()?.Value;

// 스트리밍
await foreach (var chunk in agent.InvokeStreamingAsync("안녕하세요"))
{
    // chunk 처리
}
```

### ASP.NET Core DI 통합

```csharp
// Program.cs
builder.Services.AddHiveServiceCore()
    .AddMessageGenerator("openai", new OpenAIMessageGenerator(new OpenAIConfig
    {
        ApiKey = builder.Configuration["OpenAI:ApiKey"]!
    }));

// 서비스에서 IHiveService 주입
public class ChatService(IHiveService hive)
{
    public async Task<string> ChatAsync(string text)
    {
        var agent = hive.CreateAgent(config =>
        {
            config.Model = "gpt-4o";
        });
        var response = await agent.InvokeAsync(text);
        return response.Message?.Content
            .OfType<TextMessageContent>().FirstOrDefault()?.Value ?? string.Empty;
    }
}
```

## Skills Usage

AI 코딩 에이전트(GitHub Copilot, Claude Code, Cursor 등)에서 IronHive Skills를 사용하려면:

```bash
npx skills add iyulab/ironhive
```

설치 후 에이전트가 IronHive API 패턴, 오케스트레이션, RAG 파이프라인, 툴 사용법을 자동으로 인식합니다.

## 패키지

| 패키지 | 설명 |
|--------|------|
| `IronHive.Abstractions` | 인터페이스 및 계약 |
| `IronHive.Core` | 핵심 구현 |
| `IronHive.Providers.OpenAI` | OpenAI / Azure OpenAI / xAI / GPUStack |
| `IronHive.Providers.Anthropic` | Claude 모델 |
| `IronHive.Providers.GoogleAI` | Gemini 모델 |
| `IronHive.Providers.Ollama` | 로컬 LLM (Ollama, LM Studio) |
| `IronHive.Storages.Qdrant` | Qdrant 벡터 데이터베이스 |
| `IronHive.Storages.Amazon` | Amazon S3 파일 저장소 |
| `IronHive.Storages.Azure` | Azure Blob / Service Bus |
| `IronHive.Storages.RabbitMQ` | RabbitMQ 큐 |
| `IronHive.Plugins.MCP` | Model Context Protocol |
| `IronHive.Plugins.OpenAPI` | OpenAPI 도구 통합 |

## 문서

- [아키텍처](docs/ARCHITECTURE.md) — 시스템 설계, 의존성 그래프, 확장 패턴
- [핵심 컴포넌트](docs/CORE-COMPONENTS.md) — 핵심 서비스 및 클래스 상세
- [에이전트 & 오케스트레이션](docs/AGENTS.md) — 멀티에이전트 패턴
- [프로바이더](docs/PROVIDERS.md) — AI 프로바이더 설정
- [플러그인](docs/PLUGINS.md) — MCP / OpenAPI 통합

## 요구 사항

- .NET 10.0+

## 라이선스

MIT — [LICENSE](./LICENSE) 참조.
