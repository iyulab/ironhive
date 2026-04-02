---
name: ironhive
description: IronHive is a .NET 10 enterprise AI pipeline framework. Use when building .NET AI applications with multi-provider LLM integration (OpenAI, Anthropic, GoogleAI, Ollama and OpenAI-compatible providers), multi-agent orchestration (Sequential, Parallel, GroupChat, Handoff, Graph/DAG), vector-based RAG memory pipelines, function tools, and file processing (PDF, Word, PowerPoint, Image). Covers HiveServiceBuilder setup, agent creation, streaming, [FunctionTool] attribute, and M.E.AI compatibility adapters.
license: MIT
compatibility: Requires .NET 10+
metadata:
  author: iyulab
  repo: https://github.com/iyulab/ironhive
---

# IronHive

A .NET 10 AI pipeline framework built around a **Registry + Builder** pattern.

## Packages

| Package | Purpose |
|---------|---------|
| `IronHive.Abstractions` | Interfaces and contracts only |
| `IronHive.Core` | Core implementation |
| `IronHive.Providers.OpenAI` | OpenAI / Azure OpenAI / xAI / GPUStack |
| `IronHive.Providers.Anthropic` | Anthropic Claude |
| `IronHive.Providers.GoogleAI` | Google Gemini |
| `IronHive.Providers.OpenAI.Compatible` | Ollama, DeepSeek, Groq, OpenRouter, Fireworks, Perplexity, TogetherAI, xAI |
| `IronHive.Storages.Qdrant` | Qdrant vector DB |
| `IronHive.Storages.Amazon` | Amazon S3 file storage |
| `IronHive.Storages.Azure` | Azure Blob / Service Bus |
| `IronHive.Storages.RabbitMQ` | RabbitMQ queue |
| `IronHive.Plugins.MCP` | Model Context Protocol tools |
| `IronHive.Plugins.OpenAPI` | OpenAPI tool integration |

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI   # or Anthropic, GoogleAI, etc.
```

## Setup: HiveServiceBuilder

`HiveServiceBuilder` registers providers, storages, and tools, then builds an `IHiveService`.

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "..." }))
    .AddEmbeddingGenerator("openai", new OpenAIEmbeddingGenerator(new OpenAIConfig { ApiKey = "..." }))
    .AddLocalVectorStorage("local", new LocalVectorConfig { Path = "./vectors" })
    .AddFunctionTool<MyToolClass>()
    .Build();
```

For ASP.NET Core, pass `services` to integrate with DI:

```csharp
builder.Services.AddHiveServices(hive => hive
    .AddMessageGenerator("openai", ...));
```

## Agents

Create agents via `IAgentService` or from YAML/TOML/JSON config:

```csharp
var agentService = hive.GetRequiredService<IAgentService>();

// Programmatic
var agent = agentService.CreateAgent(cfg => {
    cfg.Provider = "openai";
    cfg.Model = "gpt-4o";
    cfg.Name = "Assistant";
    cfg.Instructions = "You are a helpful assistant.";
});

// From YAML string
var agent = agentService.CreateAgentFromYaml(yamlString);

// Invoke
var response = await agent.InvokeAsync(messages);

// Streaming
await foreach (var chunk in agent.InvokeStreamingAsync(messages))
    Console.Write(chunk.Content);
```

## Orchestration

Six orchestration patterns — all implement `IAgentOrchestrator`:

| Pattern | Class | Builder | Use When |
|---------|-------|---------|----------|
| Sequential | `SequentialOrchestrator` | `OrchestratorExtensions.Sequential()` | Chain agents in order |
| Parallel | `ParallelOrchestrator` | `.Parallel()` | Run agents concurrently |
| Group Chat | `GroupChatOrchestrator` | `GroupChatOrchestratorBuilder` | Multi-turn agent discussion |
| Handoff | `HandoffOrchestrator` | `HandoffOrchestratorBuilder` | Agent routes to another agent |
| Hub-Spoke | `HubSpokeOrchestrator` | — | Central coordinator dispatches tasks |
| Graph (DAG) | `GraphOrchestrator` | `GraphOrchestratorBuilder` | Conditional branching workflows |

```csharp
// Handoff example
var orchestrator = new HandoffOrchestratorBuilder()
    .AddAgent(triageAgent, new HandoffTarget { Name = "billing" })
    .AddAgent(billingAgent)
    .SetInitialAgent("Triage")
    .Build();

var result = await orchestrator.ExecuteAsync(messages);
```

See [references/REFERENCE.md](references/REFERENCE.md) for all orchestration examples.

## Function Tools

Decorate methods with `[FunctionTool]` and register via `AddFunctionTool<T>()`:

```csharp
public class MyTools
{
    [FunctionTool(Description = "Search the web for a query")]
    public async Task<string> WebSearch(string query) { ... }

    [FunctionTool(RequiresApproval = true, Timeout = 30)]
    public string ExecuteCode(string code) { ... }
}

builder.AddFunctionTool<MyTools>();
```

Use `[FromServices]` to inject DI services and `[FromOptions]` for config into tool methods.

## RAG Memory Pipeline

```csharp
var memoryService = hive.GetRequiredService<IMemoryService>();

// Create collection
await memoryService.CreateCollectionAsync("local", "docs", "openai", "text-embedding-3-small");

// Get collection and search
var collection = await memoryService.GetCollectionAsync("local", "docs");
var results = await collection.SearchAsync("what is IronHive?", new SearchOptions { TopK = 5 });

// Build ingest worker
var worker = hive.GetRequiredService<IMemoryWorker>();
// worker.UseQueue(...).Then<TextExtractionPipeline>(...).Then<TextChunkingPipeline>(...).Build()
```

## M.E.AI Compatibility

```csharp
// Use IronHive provider as Microsoft.Extensions.AI IChatClient
var chatClient = new ChatClientAdapter(hive.GetRequiredService<IMessageService>(), "openai", "gpt-4o");

// Use as IEmbeddingGenerator<string, Embedding<float>>
var embedder = new EmbeddingGeneratorAdapter(hive.GetRequiredService<IEmbeddingService>(), "openai", "text-embedding-3-small");
```

## Messages

Core message types:

```csharp
using IronHive.Abstractions.Messages.Roles;
using IronHive.Abstractions.Messages.Content;

var messages = new List<Message>
{
    new UserMessage { Content = [new TextMessageContent { Value = "Hello" }] },
    new AssistantMessage { Content = [new TextMessageContent { Value = "Hi!" }] }
};
```

For full API reference and more examples, see [references/REFERENCE.md](references/REFERENCE.md).
