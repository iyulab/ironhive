# IronHive

<p align="center">
  <img src="assets/ironhive.png" alt="IronHive Logo" width="200"/>
</p>

<p align="center">
  <strong>Modular AI Application Framework for .NET</strong>
</p>

<p align="center">
  <a href="#features">Features</a> •
  <a href="#installation">Installation</a> •
  <a href="#quick-start">Quick Start</a> •
  <a href="#architecture">Architecture</a> •
  <a href="#providers">Providers</a> •
  <a href="#documentation">Documentation</a>
</p>

---

## Overview

**IronHive**는 .NET 기반의 모듈형 AI 애플리케이션 프레임워크입니다. 다양한 LLM 제공자를 통합하고, Vector 기반 RAG 시스템을 구축하며, 도구 호출이 가능한 에이전트 아키텍처를 지원합니다.

## Features

- **Multi-Provider LLM Integration** - OpenAI, Anthropic, Ollama, Google AI 지원
- **Vector-based RAG** - 벡터 검색 기반 지식 저장 및 검색
- **Agent Architecture** - 도구 호출 및 에이전틱 루프 지원
- **File Processing** - PDF, Word, PowerPoint, 이미지 등 다양한 파일 형식 처리
- **Memory ETL Pipeline** - 비동기 작업 처리를 위한 메모리 워커
- **Plugin System** - MCP(Model Context Protocol) 및 OpenAPI 통합

## Installation

### NuGet Packages

```bash
# Core packages
dotnet add package IronHive.Abstractions
dotnet add package IronHive.Core

# Providers
dotnet add package IronHive.Providers.OpenAI
dotnet add package IronHive.Providers.Anthropic
dotnet add package IronHive.Providers.Ollama
dotnet add package IronHive.Providers.GoogleAI

# Storage backends
dotnet add package IronHive.Storages.Qdrant
dotnet add package IronHive.Storages.AmazonS3
dotnet add package IronHive.Storages.AzureBlob

# Plugins
dotnet add package IronHive.Plugins.MCP
dotnet add package IronHive.Plugins.OpenAPI
```

## Quick Start

### Basic Setup

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

// Build HiveService with fluent API
var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", new OpenAIMessageGenerator(new OpenAIConfig
    {
        ApiKey = "your-api-key"
    }))
    .Build();

// Create an agent
var agent = hive.CreateAgent(config =>
{
    config.Provider = "openai";
    config.Model = "gpt-4o";
    config.SystemPrompt = "You are a helpful assistant.";
});

// Generate a response
var messages = new List<Message>
{
    new() { Role = "user", Content = "Hello!" }
};

var response = await agent.InvokeAsync(messages);
Console.WriteLine(response.Content);
```

### Streaming Response

```csharp
await foreach (var chunk in agent.InvokeStreamingAsync(messages))
{
    Console.Write(chunk.Content);
}
```

### Using Tools

```csharp
// Define a tool using attributes
public class WeatherTool
{
    [FunctionTool("get_weather", "Get current weather for a location")]
    public string GetWeather(
        [ToolParameter("city", "City name")] string city)
    {
        return $"The weather in {city} is sunny.";
    }
}

// Add tool to the service
var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", ...)
    .AddTool(new WeatherTool())
    .Build();
```

### RAG with Vector Storage

```csharp
using IronHive.Storages.Qdrant;

var hive = new HiveServiceBuilder()
    .AddEmbeddingGenerator("openai", new OpenAIEmbeddingGenerator(...))
    .AddVectorStorage("qdrant", new QdrantVectorStorage(new QdrantConfig
    {
        Host = "localhost",
        Port = 6334
    }))
    .Build();

// Store vectors
var collection = await hive.GetCollectionAsync("qdrant", "my-knowledge");
await collection.UpsertVectorsAsync(documents);

// Search
var results = await collection.SearchVectorsAsync(query, limit: 5);
```

## Architecture

```
IronHive/
├── IronHive.Abstractions     # Core interfaces and contracts
├── IronHive.Core             # Main implementation
│   ├── Services/             # HiveService, MessageService, etc.
│   ├── Storages/             # Local storage implementations
│   ├── Tools/                # Function tool system
│   └── Pipelines/            # ETL pipeline steps
├── IronHive.Providers.*      # LLM provider implementations
├── IronHive.Storages.*       # Storage backend implementations
└── IronHive.Plugins.*        # Extension plugins
```

### Key Components

| Component | Description |
|-----------|-------------|
| `IHiveService` | Main service aggregator |
| `IProviderRegistry` | Manages AI providers |
| `IStorageRegistry` | Manages storage backends |
| `IToolCollection` | Manages available tools |
| `IAgent` | Agent interface for message generation |
| `MemoryWorker` | Async queue-based job processor |

## Providers

### LLM Providers

| Provider | Package | Features |
|----------|---------|----------|
| OpenAI | `IronHive.Providers.OpenAI` | Chat, Embeddings, Token counting |
| Anthropic | `IronHive.Providers.Anthropic` | Claude models |
| Ollama | `IronHive.Providers.Ollama` | Local LLM support |
| Google AI | `IronHive.Providers.GoogleAI` | Gemini models |

### Storage Backends

| Type | Implementations |
|------|-----------------|
| Vector | Qdrant, Local (SQLite) |
| File | Local, Amazon S3, Azure Blob |
| Queue | Local, RabbitMQ, Azure Service Bus |

## Requirements

- .NET 10.0 or later
- C# 12.0

## License

MIT License - see [LICENSE](./LICENSE) for details.