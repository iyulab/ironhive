# IHiveService & Services

## IHiveService

```csharp
public interface IHiveService : IDisposable
{
    IModelService       Models      { get; }
    IMessageService     Messages    { get; }
    IEmbeddingService   Embeddings  { get; }
    IImageService       Images      { get; }
    IVideoService       Videos      { get; }
    IAudioService       Audio       { get; }
    IFileStorageService Files       { get; }
    IMemoryService      Memory      { get; }

    IAgent CreateAgentFrom(Action<AgentConfig> configure);
    IAgent CreateAgentFrom(AgentCard card);
    IAgent CreateAgentFromYaml(string yaml);
}

// Extension method (from HiveService)
IMemoryWorker CreateMemoryWorkerFrom(Func<MemoryWorkerBuilder, IMemoryWorker> configure);
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
    Messages        = messages,           // List<Message>
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

## IEmbeddingService — Embeddings

```csharp
public interface IEmbeddingService
{
    Task<EmbeddingResult> GenerateEmbeddingAsync(
        string provider, string model, string text,
        CancellationToken ct = default);

    Task<IEnumerable<EmbeddingResult>> GenerateEmbeddingsAsync(
        string provider, string model, IEnumerable<string> texts,
        CancellationToken ct = default);
}

// Usage
var result = await hive.Embeddings.GenerateEmbeddingAsync("openai", "text-embedding-3-small", "hello");
ReadOnlyMemory<float> vector = result.Vector;
```

## IImageService — Image Generation

```csharp
public interface IImageService
{
    Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request, CancellationToken ct = default);
    Task<ImageGenerationResponse> EditAsync(ImageEditRequest request, CancellationToken ct = default);
}

// Generate
var response = await hive.Images.GenerateAsync(new ImageGenerationRequest
{
    Provider = "openai",
    Model    = "dall-e-3",
    Prompt   = "A futuristic city at sunset",
    Size     = GeneratedImageSize.Square1024
});

foreach (var image in response.Images)
    Console.WriteLine(image.Url ?? Convert.ToBase64String(image.Data ?? []));
```

## IVideoService — Video Generation

```csharp
public interface IVideoService
{
    Task<VideoGenerationResponse> GenerateAsync(VideoGenerationRequest request, CancellationToken ct = default);
}

// Generate (async polling — Google Veo, etc.)
var response = await hive.Videos.GenerateAsync(new VideoGenerationRequest
{
    Provider = "google",
    Model    = "veo-2.0-generate-001",
    Prompt   = "A serene mountain lake at dawn",
    Size     = GeneratedVideoSize.Landscape720p
});

foreach (var video in response.Videos)
    Console.WriteLine(video.Url);
```

## IAudioService — TTS / STT

```csharp
public interface IAudioService
{
    Task<TextToSpeechResponse> TextToSpeechAsync(TextToSpeechRequest request, CancellationToken ct = default);
    Task<SpeechToTextResponse> SpeechToTextAsync(SpeechToTextRequest request, CancellationToken ct = default);
}

// TTS
var tts = await hive.Audio.TextToSpeechAsync(new TextToSpeechRequest
{
    Provider = "openai",
    Model    = "tts-1",
    Voice    = "alloy",
    Text     = "Hello from IronHive"
});
byte[] audioBytes = tts.Audio.Data;

// STT
var stt = await hive.Audio.SpeechToTextAsync(new SpeechToTextRequest
{
    Provider  = "openai",
    Model     = "whisper-1",
    AudioData = audioBytes,
    Language  = "ko"
});
Console.WriteLine(stt.Text);
```

## IModelService — Model Discovery

```csharp
public interface IModelService
{
    Task<ModelCardList> ListModelsAsync(string provider, CancellationToken ct = default);
    Task<IModelCard?> FindModelAsync(string provider, string model, CancellationToken ct = default);
}

var models = await hive.Models.ListModelsAsync("openai");
foreach (var m in models)
    Console.WriteLine($"{m.Id} — {m.Description}");
```

## IFileStorageService — File Storage

```csharp
public interface IFileStorageService
{
    Task UploadAsync(string storageName, string path, Stream data, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string storageName, string path, CancellationToken ct = default);
    Task DeleteAsync(string storageName, string path, CancellationToken ct = default);
    Task<IEnumerable<string>> ListAsync(string storageName, string? prefix = null, CancellationToken ct = default);
}

// Usage
await hive.Files.UploadAsync("s3", "documents/report.pdf", pdfStream);
var stream = await hive.Files.DownloadAsync("s3", "documents/report.pdf");
```

## IMemoryService — Vector Memory

See [MEMORY.md](MEMORY.md) for full RAG pipeline.

```csharp
await hive.Memory.CreateCollectionAsync("qdrant", "docs", "openai", "text-embedding-3-small");
var collection = await hive.Memory.GetCollectionAsync("qdrant", "docs");
var results    = await collection.SemanticSearchAsync("query", new SearchOptions { TopK = 5 });
```

## IFileParserService (separate DI registration)

Registered via `services.AddFileParser()` — independent of `IHiveService`.

```csharp
// Program.cs
builder.Services.AddFileParser();

// Usage
public class ParseService(IFileParserService parser)
{
    public async Task<string> ExtractTextAsync(Stream stream, string contentType)
    {
        var blocks = await parser.ParseAsync(stream, contentType);
        return string.Join("\n", blocks.Select(b => b.Text));
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
    public List<MessageContent> Content { get; set; }
}

// Content types (polymorphic)
TextMessageContent    { Value: string }
ImageMessageContent   { MediaType: string, Data: string (base64) | Url: string }
ToolMessageContent    { /* tool call / result */ }
ThinkingMessageContent { Thinking: string }          // Anthropic Extended Thinking

// Build multimodal message
var msg = new Message
{
    Role = MessageRole.User,
    Content =
    [
        new TextMessageContent { Value = "Describe this image" },
        new ImageMessageContent { MediaType = "image/png", Data = Convert.ToBase64String(bytes) }
    ]
};
```

## M.E.AI Compatibility

```csharp
// Use as Microsoft.Extensions.AI IChatClient
var chatClient = new ChatClientAdapter(hive.Messages, "openai", "gpt-4o");

// Use as IEmbeddingGenerator<string, Embedding<float>>
var embedder = new EmbeddingGeneratorAdapter(hive.Embeddings, "openai", "text-embedding-3-small");

// Use function tool as AITool
var aiTool = new AIToolAdapter(myTool);
```

## Telemetry

```csharp
// OpenTelemetry integration
// ActivitySource: "IronHive"
// Meter:          "IronHive"

services.AddOpenTelemetry()
    .WithTracing(b => b.AddSource("IronHive"))
    .WithMetrics(b => b.AddMeter("IronHive"));
```
