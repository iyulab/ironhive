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

A modular AI application framework for .NET. Multi-provider LLM integration, vector-based RAG pipeline, multi-agent orchestration, and file processing — with a fluent builder API.

## Features

- **Multi-Provider LLM** — OpenAI, Anthropic, Google AI, Ollama
- **Multi-Agent Orchestration** — Sequential, Parallel, Hub-Spoke, Graph (DAG)
- **RAG Pipeline** — Text extraction, chunking, embedding, vector search
- **File Processing** — PDF, Word, PowerPoint, images
- **Plugin System** — MCP and OpenAPI integration
- **M.E.AI Compatible** — `ChatClientAdapter` / `EmbeddingGeneratorAdapter`

## Installation

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI    # or Anthropic, GoogleAI, Ollama
```

## Quick Start

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", new OpenAIMessageGenerator(new OpenAIConfig
    {
        ApiKey = "your-api-key"
    }))
    .Build();

var agent = hive.CreateAgent(config =>
{
    config.Provider = "openai";
    config.Model = "gpt-4o";
    config.SystemPrompt = "You are a helpful assistant.";
});

var response = await agent.InvokeAsync(messages);

// Streaming
await foreach (var chunk in agent.InvokeStreamingAsync(messages))
{
    Console.Write(chunk.Content);
}
```

## Packages

| Package | Description |
|---------|-------------|
| `IronHive.Abstractions` | Interfaces and contracts |
| `IronHive.Core` | Core implementation |
| `IronHive.Providers.OpenAI` | OpenAI / Azure OpenAI / xAI / GPUStack |
| `IronHive.Providers.Anthropic` | Claude models |
| `IronHive.Providers.GoogleAI` | Gemini models |
| `IronHive.Providers.Ollama` | Local LLM (Ollama, LM Studio) |
| `IronHive.Storages.Qdrant` | Qdrant vector database |
| `IronHive.Storages.Amazon` | Amazon S3 file storage |
| `IronHive.Storages.Azure` | Azure Blob / Service Bus |
| `IronHive.Storages.RabbitMQ` | RabbitMQ queue |
| `IronHive.Plugins.MCP` | Model Context Protocol |
| `IronHive.Plugins.OpenAPI` | OpenAPI tool integration |

## Documentation

- [Architecture](docs/ARCHITECTURE.md) — System design, dependency graph, extension patterns
- [Design Notes](docs/DESIGN.md) — Builder/service API, orchestration patterns
- [Project Structure](docs/STRUCTURE.md) — Service classification and lifetimes

## Requirements

- .NET 10.0+

## License

MIT — see [LICENSE](./LICENSE).
