# STRUCTURE

## Abstraction

```yaml
- IHiveMind
  - CreateSession()
  - 

- IHiveSession
  - Title
  - Summary
  - TokenUsage
  - InvokeAsync()
  - InvokeStreamingAsync()

- IHiveServiceContainer
  - Connectors
  - StorageManager

```

## Core

```yaml
- ChatCompletionService
  - ExecuteAsync()
  - ExecuteStreamingAsync()
- EmbeddingService
  - EmbedAsync()
  - EmbedBatchAsync()
```

