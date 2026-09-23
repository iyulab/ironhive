# IHiveService & Services

## IHiveService

```csharp
public interface IHiveService : IDisposable
{
    IModelService       Models      { get; }
    IMessageService     Messages    { get; }
    IEmbeddingService   Embeddings  { get; }
    IRerankService      Rerank      { get; }
    IImageService       Images      { get; }
    IVideoService       Videos      { get; }
    IAudioService       Audio       { get; }
    IFileStorageService Files       { get; }
    IMemoryService      Memory      { get; }

    IAgent CreateAgentFrom(Action<AgentConfig> configure);
    IAgent CreateAgentFrom(AgentCard card);
    IAgent CreateAgentFromYaml(string yaml);
}

// Extension method (IronHive.Core)
IMemoryWorker CreateMemoryWorkerFrom(this IHiveService service, Func<MemoryWorkerBuilder, MemoryPipelineBuilder> configure, IServiceProvider? sp = null);
```

## IMessageService — LLM Chat

```csharp
public interface IMessageService
{
    Task<MessageResponse> GenerateMessageAsync(
        MessageRequest request,
        CancellationToken ct = default);

    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageRequest request,
        CancellationToken ct = default);
}

// Request
var request = new MessageRequest
{
    Provider        = "openai",
    Model           = "gpt-4o-mini",
    System          = "You are helpful.",
    Messages        = messages,           // ICollection<Message>
    Tools           = toolCollection,     // optional IToolCollection
    ThinkingEffort  = MessageThinkingEffort.High,   // Extended thinking
    PreviousId      = "prev-response-id"            // Responses API continuity
};

var response = await hive.Messages.GenerateMessageAsync(request);
```

### Tool-Use Loop

`MessageService` automatically handles tool calls:
1. LLM returns `tool_use` → execute matching tools (up to 3 in parallel)
2. Append results as `ToolMessage` → re-send
3. Repeat until `stop` response

To intercept individual tool invocations (adjust input, transform output, mock), use `MessageRequest.ToolOptions.OnBeforeInvoke`/`OnAfterInvoke` — see [TOOLS.md](TOOLS.md).

### IMessageMiddleware

Next-chain middleware wrapping the generator call in each turn (not the whole multi-turn `IMessageService` call, unlike `IAgentMiddleware` — see [MIDDLEWARE.md](MIDDLEWARE.md)). Use for per-turn concerns: compaction, retry, error handling.

```csharp
public interface IMessageMiddleware
{
    Task<MessageResponse> GenerateAsync(
        MessageContext context,
        Func<MessageContext, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default);   // default: pass-through

    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingAsync(
        MessageContext context,
        Func<MessageContext, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default);   // default: pass-through
}

// Registration — global, applied outer-to-inner in registration order
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", config)
    .AddMessageMiddleware(new CompactingMiddleware())
    .Build();
```

`MessageContext` lives for the whole `MessageService` call (all turns):

| Member | Description |
|--------|-------------|
| `Request` | Outgoing `MessageGenerationRequest` for this turn; `Messages` accumulates across turns. |
| `CurrentTurn` / `MaxTurns` | Current turn index (0-based) and the max turns allowed this call. |
| `CurrentMessage` | Assistant message accumulated across turns; null until the first content arrives. |
| `TrackedId` / `TurnReason` / `TokenUsage` | Most recent turn's raw (unprefixed) response ID / stop reason / token usage. |
| `Items` | Shared data across pipeline stages. Seeded from `MessageRequest.Items`, flows out via `MessageResponse.Items` (or `StreamingMessageDoneResponse.Items` when streaming). |

Wrapping `next()` in try/catch gives retry/fallback; inspecting/mutating `context.Request` before calling `next()` gives pre-turn compaction/mutation.

### ContextOverflowException

Provider-specific context-window overflow errors (OpenAI, Anthropic, GoogleAI, OpenAI Compatible) are normalized to `ContextOverflowException` (`IronHive.Abstractions.Exceptions`) — `ContextWindow` (nullable `int`) when the provider reports it.

```csharp
try
{
    var response = await hive.Messages.GenerateMessageAsync(request);
}
catch (ContextOverflowException ex)
{
    // summarize/trim request.Messages and retry
}
```

## IEmbeddingService — Embeddings

```csharp
public interface IEmbeddingService
{
    IReadOnlyDictionary<string, IEmbeddingGenerator> Generators { get; }

    Task<float[]> EmbedAsync(
        string provider, string modelId, string input,
        CancellationToken ct = default);

    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider, string modelId, IEnumerable<string> inputs,
        CancellationToken ct = default);

    Task<int> CountTokensAsync(
        string provider, string modelId, string input,
        CancellationToken ct = default);
}

// Usage
float[] vector = await hive.Embeddings.EmbedAsync("openai", "text-embedding-3-small", "hello");

var batch = await hive.Embeddings.EmbedBatchAsync("openai", "text-embedding-3-small", texts);
foreach (var r in batch)
    Console.WriteLine($"{r.Index}: {r.Embedding?.Length}");
```

## IImageService — Image Generation

```csharp
public interface IImageService
{
    Task<ImageGenerationResponse> GenerateImageAsync(string provider, ImageGenerationRequest request, CancellationToken ct = default);
    Task<ImageGenerationResponse> EditImageAsync(string provider, ImageEditRequest request, CancellationToken ct = default);
}

// Generate — the provider is a method argument, not a request field
var response = await hive.Images.GenerateImageAsync("openai", new ImageGenerationRequest
{
    Model  = "dall-e-3",
    Prompt = "A futuristic city at sunset",
    Size   = new GeneratedImagePixelSize { Width = 1024, Height = 1024 }
    // or: new GeneratedImageScaleSize { Resolution = "1k", AspectRatio = "1:1" }
});

foreach (var image in response.Images)
    File.WriteAllBytes("out.png", image.Data);   // results are bytes (image.ToBase64() for text)
```

## IVideoService — Video Generation

```csharp
public interface IVideoService
{
    Task<VideoGenerationResponse> GenerateVideoAsync(
        string provider, VideoGenerationRequest request,
        IProgress<VideoGenerationProgress>? progress = null, CancellationToken ct = default);
}

// Generate (async polling — Google Veo, etc.)
var response = await hive.Videos.GenerateVideoAsync("google", new VideoGenerationRequest
{
    Model  = "veo-2.0-generate-001",
    Prompt = "A serene mountain lake at dawn",
    Size   = new GeneratedVideoPresetSize { Resolution = "720p", AspectRatio = "16:9" }
});

File.WriteAllBytes("out.mp4", response.Video.Data);   // a single video, as bytes
```

## IAudioService — TTS / STT

```csharp
public interface IAudioService
{
    IReadOnlyDictionary<string, IAudioProcessor> Processors { get; }
    Task<TextToSpeechResponse> GenerateSpeechAsync(string provider, TextToSpeechRequest request, CancellationToken ct = default);
    Task<SpeechToTextResponse> TranscribeAsync(string provider, SpeechToTextRequest request, CancellationToken ct = default);
}

// TTS
var tts = await hive.Audio.GenerateSpeechAsync("openai", new TextToSpeechRequest
{
    Model = "tts-1",
    Voice = "alloy",
    Text  = "Hello from IronHive"
});
byte[] audioBytes = tts.Audio.Data;          // GeneratedAudio { MimeType, Data }

// STT — the input audio is wrapped in GeneratedAudio
var stt = await hive.Audio.TranscribeAsync("openai", new SpeechToTextRequest
{
    Model = "whisper-1",
    Audio = new GeneratedAudio { Data = audioBytes, MimeType = "audio/mpeg" }
});
Console.WriteLine(stt.Text);
foreach (var seg in stt.Segments ?? [])      // segments when the provider returns them (e.g. Diarized = true)
    Console.WriteLine($"  [{seg.Start:F1}s] {seg.Speaker}: {seg.Text}");
```

## IModelService — Model Discovery

```csharp
public interface IModelService
{
    Task<IEnumerable<ModelCardList>> ListModelsAsync(CancellationToken ct = default);   // every provider
    Task<ModelCardList?> ListModelsAsync(string provider, CancellationToken ct = default);
    Task<IModelCard?> FindModelAsync(string provider, string modelId, CancellationToken ct = default);
}

var list = await hive.Models.ListModelsAsync("openai");
foreach (var m in list?.Models ?? [])
    Console.WriteLine($"{m.ModelId} — {m.Description}");
```

## IFileStorageService — File Storage

```csharp
public interface IFileStorageService
{
    Task<IEnumerable<string>> ListAsync(string storageName, string? prefix = null, int depth = 1, CancellationToken ct = default);
    Task<bool> ExistsFileAsync(string storageName, string filePath, CancellationToken ct = default);
    Task<Stream> ReadFileAsync(string storageName, string filePath, CancellationToken ct = default);
    Task WriteFileAsync(string storageName, string filePath, Stream data, bool overwrite = true, CancellationToken ct = default);
    Task DeleteFileAsync(string storageName, string filePath, CancellationToken ct = default);
    Task DeleteDirectoryAsync(string storageName, string directoryPath, CancellationToken ct = default);
}

// Usage
await hive.Files.WriteFileAsync("s3", "documents/report.pdf", pdfStream);
var stream = await hive.Files.ReadFileAsync("s3", "documents/report.pdf");
```

## IMemoryService — Vector Memory

See [MEMORY.md](MEMORY.md) for full RAG pipeline.

```csharp
await hive.Memory.CreateCollectionAsync("qdrant", "docs", "openai", "text-embedding-3-small");
var collection = await hive.Memory.GetCollectionAsync("qdrant", "docs");
var results    = await collection.SemanticSearchAsync("query", new SearchOptions { Limit = 5 });
```

## IFileParserService (separate DI registration)

Registered via `services.AddFileParser()` — independent of `IHiveService`.

```csharp
// Program.cs
builder.Services.AddFileParser();

// Usage
public class ParseService(IFileParserService parser)
{
    public async Task<string> ExtractTextAsync(string fileName, Stream data)
    {
        var blocks = await parser.ParseAsync(fileName, data);   // TextBlock | ImageBlock
        return string.Join("\n", blocks.OfType<TextBlock>().Select(b => b.Text));
    }
}
```

Supported: PDF (`.pdf`), Word (`.docx`), Excel (`.xlsx`), PowerPoint (`.pptx`), Images (`.png`, `.jpg`, etc.), plain text

## Message Types

```csharp
// Message
public class Message
{
    public MessageRole Role { get; set; }           // User | Assistant
    public ICollection<MessageContent> Content { get; set; }
}

// Content types (polymorphic)
TextMessageContent     { Value: string }
ImageMessageContent    { Format: ImageFormat, Base64: string }
ToolMessageContent     { /* tool call / result */ }
ThinkingMessageContent { Value: string }             // extended thinking / reasoning

// Build multimodal message
var msg = new Message
{
    Role = MessageRole.User,
    Content =
    [
        new TextMessageContent { Value = "Describe this image" },
        new ImageMessageContent { Format = ImageFormat.Png, Base64 = Convert.ToBase64String(bytes) }
    ]
};
```

## M.E.AI Compatibility

```csharp
// ChatClientAdapter/EmbeddingGeneratorAdapter wrap a single provider's raw IMessageGenerator/
// IEmbeddingGenerator. To reuse a provider registered via HiveServiceBuilder without
// reconstructing it, pull it out of the service's Generators dictionary — GetOrFirstValue
// auto-selects the sole registered provider when unspecified, throws if more than one.

// Use as Microsoft.Extensions.AI IChatClient
var chatClient = hive.Messages.Generators.GetOrFirstValue("openai")
    .AsChatClient("gpt-4o", "openai");

// Use as IEmbeddingGenerator<string, Embedding<float>>
var embedder = hive.Embeddings.Generators.GetOrFirstValue("openai")
    .AsEmbeddingGenerator("text-embedding-3-small", "openai");

// Wrap an AITool (e.g. an MCP McpClientTool) as an IronHive ITool — InvokeAsync executes it
var tool = new AIToolAdapter(mcpClientTool);
```

## Telemetry

```csharp
// OpenTelemetry integration (OpenTelemetry.Extensions.Hosting package)
// ActivitySource and Meter are both named HiveTelemetry.SourceName ("IronHive")

services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource(HiveTelemetry.SourceName))
    .WithMetrics(b => b.AddMeter(HiveTelemetry.SourceName));
```
