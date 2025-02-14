# STRUCTURE

- IMessageSession(µ¶¸³)
  - Title
  - Summary
  - TruncatedIndex
  - TokenUsage
  - Invoke()

- Task(Workflow)
  - ID
  - Nodes(Action)
    - ID
    - Execute()
  - Edges
    - From
    - To

- IHiveMind
  - IManager
  - IAssistant(IWorker)
    - Name
    - Description
    - Instruction
    - Tools
    - AddTool()
  - AddAssistant()
  - Invoke()

- ToolManager(??)
  - Tools
  - RetryPolicy
  - AddTool()
  - RemoveTool()
  - ChainingTools()
  - Invoke()

- IChatCompletionService
  - GenerateCompletion()
- IEmbeddingService
  - GetEmbedding()
