# IronHive Reference

## All Provider Registrations

```csharp
var hive = new HiveServiceBuilder()
    // OpenAI
    .AddMessageGenerator("openai", new OpenAIMessageGenerator(new OpenAIConfig { ApiKey = "..." }))
    .AddEmbeddingGenerator("openai", new OpenAIEmbeddingGenerator(new OpenAIConfig { ApiKey = "..." }))

    // Anthropic
    .AddMessageGenerator("anthropic", new AnthropicMessageGenerator(new AnthropicConfig { ApiKey = "..." }))

    // Google AI
    .AddMessageGenerator("google", new GoogleAIMessageGenerator(new GoogleAIConfig { ApiKey = "..." }))

    // Ollama (OpenAI-compatible)
    .AddMessageGenerator("ollama", new OllamaMessageGenerator(new OllamaConfig { BaseUrl = "http://localhost:11434" }))

    // Other OpenAI-compatible: DeepSeek, Groq, OpenRouter, Fireworks, Perplexity, TogetherAI, xAI
    .AddMessageGenerator("groq", new GroqMessageGenerator(new GroqConfig { ApiKey = "..." }))

    .Build();
```

## Agent YAML Config

```yaml
agent:
  name: "SupportBot"
  description: "Handles customer inquiries"
  defaultProvider: "openai"
  defaultModel: "gpt-4o-mini"
  instructions: |
    You are a customer support agent. Be concise.
  tools: ["web-search", "calculator"]
  toolOptions:
    web-search:
      region: "KR"
  parameters:
    maxTokens: 512
    temperature: 0.3
    topP: 0.9
```

## Agent TOML Config

```toml
[agent]
name = "SupportBot"
defaultProvider = "openai"
defaultModel = "gpt-4o-mini"
instructions = "You are a customer support agent."
tools = ["web-search"]

[agent.parameters]
maxTokens = 512
temperature = 0.3
```

## Orchestration Examples

### Sequential

```csharp
var orchestrator = OrchestratorExtensions.Sequential()
    .AddAgent(researchAgent)
    .AddAgent(summaryAgent)
    .Build();

var result = await orchestrator.ExecuteAsync(messages);
```

### Group Chat

```csharp
var orchestrator = new GroupChatOrchestratorBuilder()
    .AddAgent(agentA)
    .AddAgent(agentB)
    .WithLlmManager(managerAgent)       // or .WithRoundRobin() / .WithRandom()
    .TerminateOnKeyword("TERMINATE")    // or .TerminateAfterRounds(10)
    .SetTimeout(TimeSpan.FromMinutes(10))
    .Build();

var result = await orchestrator.ExecuteAsync(messages);
```

### Handoff

```csharp
var orchestrator = new HandoffOrchestratorBuilder()
    .AddAgent(triageAgent,
        new HandoffTarget { Name = "Billing", Condition = "billing-related" },
        new HandoffTarget { Name = "Technical", Condition = "tech-related" })
    .AddAgent(billingAgent)
    .AddAgent(technicalAgent)
    .SetInitialAgent("Triage")
    .SetMaxTransitions(20)
    .Build();
```

### Graph (DAG)

```csharp
var orchestrator = new GraphOrchestratorBuilder()
    .AddNode("classify", classifyAgent)
    .AddNode("billing", billingAgent)
    .AddNode("general", generalAgent)
    .AddEdge("classify", "billing", result => result.Output?.Contains("billing") == true)
    .AddEdge("classify", "general")
    .SetStartNode("classify")
    .SetOutputNode("billing")
    .Build();
```

## Checkpoint / Resumable Orchestration

```csharp
// File-based persistence
var store = new FileCheckpointStore("./checkpoints");

// In-memory
var store = new InMemoryCheckpointStore();

var orchestrator = new HandoffOrchestratorBuilder()
    // ...
    .WithCheckpointStore(store)
    .SetOrchestrationId("session-123")
    .Build();
```

## RAG: Full Pipeline

```csharp
var worker = hive.CreateMemoryWorker()
    .UseQueue("local-queue")
    .Then<TextExtractionPipeline>("extract")
    .Then<TextChunkingPipeline, TextChunkingOptions>("chunk", new TextChunkingOptions
    {
        ChunkSize = 512,
        Overlap = 50
    })
    .Then<CreateVectorsPipeline>("embed")
    .Then<StoreVectorsPipeline>("store")
    .Build();

// Enqueue document for ingestion
await worker.EnqueueAsync(new MemoryContext
{
    StorageName = "local",
    CollectionName = "docs",
    FilePath = "./document.pdf"
});

// Search
var collection = await memoryService.GetCollectionAsync("local", "docs");
var hits = await collection.SearchAsync("query text", new SearchOptions
{
    TopK = 5,
    MinScore = 0.7f
});
foreach (var hit in hits)
    Console.WriteLine($"[{hit.Score:F2}] {hit.Record.Content}");
```

## Function Tool: Full Options

```csharp
public class MyTools
{
    private readonly HttpClient _http;

    public MyTools([FromServices] HttpClient http) => _http = http;

    [FunctionTool(Name = "web_search", Description = "Search the web")]
    public async Task<string> Search(string query, int maxResults = 5)
    {
        // implementation
    }

    [FunctionTool(RequiresApproval = true, Timeout = 120)]
    public string RunScript([FromOptions("script-runner")] ScriptOptions opts, string code)
    {
        // implementation
    }
}

// Register
builder.AddFunctionTool<MyTools>();
```

## Agent Middleware

```csharp
// Built-in middleware: Retry, Fallback, Bulkhead, RateLimit, Timeout, CircuitBreaker, Caching, Logging
var agentWithMiddleware = agent
    .WithRetry(maxAttempts: 3)
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithLogging(logger);
```

## Resilience

```csharp
var hive = new HiveServiceBuilder()
    .AddMessageGenerator("openai", generator, new ResilienceOptions
    {
        RetryCount = 3,
        RetryDelay = TimeSpan.FromSeconds(2),
        CircuitBreakerThreshold = 5
    })
    .Build();
```

## Streaming with State Management

```csharp
// Resume interrupted streams
var stateManager = hive.GetRequiredService<IStreamStateManager>();
var streamId = Guid.NewGuid().ToString();

await foreach (var chunk in agent.InvokeStreamingAsync(messages))
{
    await stateManager.AppendAsync(streamId, chunk);
    Console.Write(chunk.Content);
}
```

## Message Content Types

```csharp
new UserMessage
{
    Content =
    [
        new TextMessageContent { Value = "Describe this image" },
        new ImageMessageContent { Format = ImageFormat.Jpeg, Base64 = Convert.ToBase64String(bytes) },
        new DocumentMessageContent { FileName = "report.pdf", Base64 = Convert.ToBase64String(pdfBytes) }
    ]
}
```

## File Services

```csharp
var extractionService = hive.GetRequiredService<IFileExtractionService<string>>();

// Extract text from PDF, Word, PPT, Image
var text = await extractionService.ExtractAsync("document.pdf");

// Upload to storage
var fileService = hive.GetRequiredService<IFileStorageService>();
await fileService.UploadAsync("local", "files/doc.pdf", stream);
```

## Image / Audio / Video

```csharp
// Image generation
var imageService = hive.GetRequiredService<IImageService>();
var result = await imageService.GenerateAsync(new ImageGenerationRequest
{
    Provider = "openai",
    Model = "dall-e-3",
    Prompt = "A futuristic city"
});

// Text-to-Speech
var audioService = hive.GetRequiredService<IAudioService>();
var audio = await audioService.TextToSpeechAsync(new TextToSpeechRequest
{
    Provider = "openai",
    Model = "tts-1",
    Text = "Hello, world!"
});

// Speech-to-Text
var stt = await audioService.SpeechToTextAsync(new SpeechToTextRequest
{
    Provider = "openai",
    Model = "whisper-1",
    AudioData = audioBytes
});
```

## Plugins

### MCP

```csharp
using IronHive.Plugins.MCP;

builder.AddMcpTools("my-mcp-server", new McpConfig { Command = "npx", Args = ["-y", "@my/mcp-server"] });
```

### OpenAPI

```csharp
using IronHive.Plugins.OpenAPI;

builder.AddOpenApiTools("petstore", new OpenApiConfig { Url = "https://petstore.swagger.io/v2/swagger.json" });
```
