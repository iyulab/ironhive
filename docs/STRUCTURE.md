<title>
  good
</title>

# STRUCTURE

## Core

```yaml
- IHive

- ChatAgent : IAgent<MessageRequest, MessageRespone>
  - ID
  - Name
  - Description
  - Instruction
  - InvokeAsync()
  - InvokeStreamingAsync()

- SummaryAgent : IAgent<string,string>
  - ID
  - Name
  - Description
  - Instruction
  - InvokeAsync()

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

