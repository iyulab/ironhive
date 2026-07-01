---
name: ironhive
description: IronHive is a .NET 10 enterprise AI pipeline framework. Use when building .NET AI applications with multi-provider LLM integration (OpenAI, Anthropic, GoogleAI, Vertex AI, OpenAI-compatible), multi-agent orchestration (Sequential, Parallel, Handoff, GroupChat, HubSpoke, Graph/DAG), vector-based RAG memory pipelines, function tools with [FunctionTool] attribute, MCP/OpenAPI plugins, file parsing, and M.E.AI compatibility adapters.
license: MIT
compatibility: Requires .NET 10+
metadata:
  author: iyulab
  repo: https://github.com/iyulab/ironhive
---

# IronHive

A .NET 10 AI pipeline framework built on a **Named Registry + Builder** pattern. All providers, storages, and services are registered by name and resolved at runtime via `IHiveService`.

## Quick Reference Index

| Topic | Reference File |
|-------|---------------|
| Setup & DI registration | [references/SETUP.md](references/SETUP.md) |
| Agent creation & invocation | [references/AGENTS.md](references/AGENTS.md) |
| Middleware (Retry, Timeout, etc.) | [references/MIDDLEWARE.md](references/MIDDLEWARE.md) |
| Multi-agent orchestration | [references/ORCHESTRATION.md](references/ORCHESTRATION.md) |
| Function tools & tool collection | [references/TOOLS.md](references/TOOLS.md) |
| RAG memory pipeline | [references/MEMORY.md](references/MEMORY.md) |
| IHiveService services (chat, embed, image…) | [references/SERVICES.md](references/SERVICES.md) |
| AI provider configurations | [references/PROVIDERS.md](references/PROVIDERS.md) |
| Storage backends | [references/STORAGES.md](references/STORAGES.md) |

## Minimal Example

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." })
    .Build();

var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider = "openai";
    cfg.Model    = "gpt-4o-mini";
    cfg.Instructions = "You are a helpful assistant.";
});

var response = await agent.InvokeAsync("Hello!");
```

## Key Design Principles

- `HiveServiceBuilder` → `IHiveService` (builder pattern, all registrations by name string)
- Agents are created from `IHiveService`, not from DI
- Tools live on `IAgent.Tools` (`IToolCollection`), not on the builder
- Storage and provider names are arbitrary strings — you choose them and reference them later
- All services accessed via `hive.Models`, `hive.Messages`, `hive.Embeddings`, `hive.Images`, `hive.Videos`, `hive.Audio`, `hive.Files`, `hive.Memory`
