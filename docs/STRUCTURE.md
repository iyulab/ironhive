# STRUCTURE

## Core

```yaml
- IHiveMind
  - IAgent[]
  - InvokeAsync()

- IChatbot : IAgent
  - ID
  - Name
  - Description
  - Instruction
  - ExecuteAsync()
  - ExecuteStreamingAsync()

- IWorker<TIn,TOut> : IAgent
  - ID
  - Name
  - Description
  - Instruction
  - TOut ExecuteAsync(TIn)

- IMessageSession
  - Title
  - CondensationStrategy
  - TokenUsage
  - ToolRetryPolicy
  - InvokeAsync()
  - InvokeStreamingAsync()

- Workflow
  - ID
  - Node[]
    - ID
    - Execute()
  - Edge[]
    - Source
    - Target
```

## Basic

```yaml
- IChatCompletionService
  - GenerateMessageAsync()
  - GenerateStreamingMessageAsync()
- IEmbeddingService
  - EmbedAsync()
  - EmbedBatchAsync()
```

