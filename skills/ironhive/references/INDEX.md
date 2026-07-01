# IronHive References Index

Quick lookup for IronHive APIs. Each file covers one area of the framework.

| File | Contents |
|------|----------|
| [SETUP.md](SETUP.md) | HiveServiceBuilder, provider registration, storage registration, DI integration |
| [AGENTS.md](AGENTS.md) | IAgent interface, CreateAgentFrom, YAML/TOML/JSON config, InvokeAsync, streaming |
| [MIDDLEWARE.md](MIDDLEWARE.md) | WithMiddleware(), Retry, Timeout, RateLimit, CircuitBreaker, Bulkhead, Caching, Logging, Fallback |
| [ORCHESTRATION.md](ORCHESTRATION.md) | Sequential, Parallel, Handoff, GroupChat, HubSpoke, Graph orchestrators; AsAgent(); checkpoints |
| [TOOLS.md](TOOLS.md) | [FunctionTool] attribute, IToolCollection, MCP tools, OpenAPI tools, custom ITool |
| [MEMORY.md](MEMORY.md) | IMemoryService, IMemoryCollection, MemoryWorker, RAG pipelines, SemanticSearchAsync |
| [SERVICES.md](SERVICES.md) | IHiveService, IMessageService, IEmbeddingService, IImageService, IAudioService, IVideoService, IFileStorageService, M.E.AI adapters |
| [PROVIDERS.md](PROVIDERS.md) | OpenAI, Anthropic, GoogleAI, VertexAI, OpenAI Compatible, GPUStack configs |
| [STORAGES.md](STORAGES.md) | IFileStorage, IVectorStorage, IQueueStorage; local/S3/Azure/Qdrant/RabbitMQ backends |
