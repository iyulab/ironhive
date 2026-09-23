# RAG Memory Pipeline

## Architecture

```
Document/File → TextExtraction → TextChunking → CreateVectors → StoreVectors
                                                      ↑
                                          IEmbeddingService (named provider)
                                                      ↓
                                              IVectorStorage (named)
```

`MemoryWorker` processes items from a queue asynchronously; agents query via `SemanticSearchAsync`.

## Collection Management (hive.Memory)

```csharp
// Create collection (specifies which vector storage + embedding provider)
await hive.Memory.CreateCollectionAsync(
    storageName:       "qdrant",
    collectionName:    "documents",
    embeddingProvider: "openai",
    embeddingModel:    "text-embedding-3-small");

// Check existence
bool exists = await hive.Memory.CollectionExistsAsync("qdrant", "documents");

// List collections
var list = await hive.Memory.ListCollectionsAsync("qdrant", prefix: "docs-");

// Delete collection
await hive.Memory.DeleteCollectionAsync("qdrant", "documents");

// Get collection handle
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");
```

## IMemoryService Interface

```csharp
public interface IMemoryService
{
    Task<IMemoryCollection> GetCollectionAsync(string storageName, string collectionName, CancellationToken ct = default);
    Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(string storageName, string? prefix = null, CancellationToken ct = default);
    Task<bool> CollectionExistsAsync(string storageName, string collectionName, CancellationToken ct = default);
    Task CreateCollectionAsync(string storageName, string collectionName, string embeddingProvider, string embeddingModel, CancellationToken ct = default);
    Task DeleteCollectionAsync(string storageName, string collectionName, CancellationToken ct = default);
}
```

## IMemoryCollection Interface

```csharp
public interface IMemoryCollection
{
    string StorageName { get; }
    string CollectionName { get; }
    string EmbeddingProvider { get; }
    string EmbeddingModel { get; }

    // Queue a source for ingestion (async processing by MemoryWorker)
    Task IndexSourceAsync(string queueName, IMemorySource source, CancellationToken ct = default);

    // Remove all vectors for a source
    Task DeindexSourceAsync(string sourceId, CancellationToken ct = default);

    // Vector similarity search
    Task<VectorSearchResult> SemanticSearchAsync(string query, SearchOptions? options = null, CancellationToken ct = default);
}
```

## Indexing Documents

```csharp
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");

// File source — StorageName is the file storage the file is read from
await collection.IndexSourceAsync("local-queue", new FileMemorySource
{
    Id          = "doc-001",
    StorageName = "local-files",
    FilePath    = "./documents/manual.pdf"
});

// Text source
await collection.IndexSourceAsync("local-queue", new TextMemorySource
{
    Id    = "text-001",
    Value = "IronHive is a .NET 10 AI framework."
});

// Remove a source
await collection.DeindexSourceAsync("doc-001");
```

`Id` defaults to a new GUID; set it yourself for any source you will later remove, since
`DeindexSourceAsync` is keyed by it.

## MemoryWorker

```csharp
var worker = hive.CreateMemoryWorkerFrom(builder =>
    builder
        .UseQueue("local-queue")                               // IQueueStorage name
        .Then<TextExtractionPipeline>("extract")               // extract text from file
        .Then<TextChunkingPipeline, TextChunkingPipeline.Options>("chunk",
            new TextChunkingPipeline.Options(
                ChunkSize:    512,     // token-based chunk size
                ChunkOverlap: 50))     // overlap between chunks
        .Then<CreateVectorsPipeline>("embed")                  // generate embeddings
        .Then<StoreVectorsPipeline>("store"));                 // save to vector storage — CreateMemoryWorkerFrom calls Build()

// Start background processing
await worker.StartAsync();

// Monitor progress — WorkflowEventArgs<MemoryContext>
worker.Progressed += (_, args) =>
{
    // Type: Started | Progressed | Completed | Failed | Cancelled
    Console.WriteLine($"[{args.StepName}] {args.Type}: {args.Context.Source.Id}");
};

// Stop (graceful = wait for current item)
await worker.StopAsync(force: false);
await worker.StopAsync(force: true);   // immediate
```

## ASP.NET Core (BackgroundService)

```csharp
// Program.cs
builder.Services.AddHostedService<MemoryIngestionService>();

public class MemoryIngestionService(IHiveService hive) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var worker = hive.CreateMemoryWorkerFrom(b =>
            b.UseQueue("tasks")
             .Then<TextExtractionPipeline>("extract")
             .Then<TextChunkingPipeline, TextChunkingPipeline.Options>("chunk",
                 new TextChunkingPipeline.Options(ChunkSize: 512, ChunkOverlap: 50))
             .Then<CreateVectorsPipeline>("embed")
             .Then<StoreVectorsPipeline>("store"));

        await worker.StartAsync();
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            await worker.StopAsync();
        }
    }
}
```

## Semantic Search

```csharp
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");

var results = await collection.SemanticSearchAsync(
    "machine learning applications",
    new SearchOptions
    {
        Limit     = 10,           // number of results (default: 5)
        MinScore  = 0.7f,         // minimum similarity score 0–1
        SourceIds = ["doc-001"]   // optional: restrict to these sources
    });

// VectorSearchResult { CollectionName, Query, Results } — each result is a ScoredVectorRecord
foreach (var hit in results.Results)
{
    Console.WriteLine($"[{hit.Score:F3}] {hit.SourceId}");
    if (hit.Payload.TryGetValue("text", out var text))
        Console.WriteLine($"  {text}");
}
```

The record body lives in `Payload`; its keys are set by the pipeline that stored it —
`CreateVectorsPipeline` writes `text` for chunks and `question`/`answer` for dialogues.

## Built-in Pipelines

| Pipeline | Description |
|----------|-------------|
| `TextExtractionPipeline` | Extract text from PDF, DOCX, XLSX, PPTX, images, plain text |
| `TextChunkingPipeline` | Split text into semantic chunks (paragraph→sentence→clause→word) |
| `CreateVectorsPipeline` | Generate embeddings via configured provider |
| `StoreVectorsPipeline` | Store vectors in IVectorStorage |
| `DialogueExtractionPipeline` | Extract dialogue-formatted text |

## Custom Pipelines

A pipeline mutates `MemoryContext` in place and returns only success/failure. Data passes between
steps through `context.Payload` (keys used by the built-ins: `text`, `chunks`, `vectors`).

```csharp
// Without options
public class MyFilterPipeline : IMemoryPipeline
{
    public Task<TaskStepResult> ExecuteAsync(MemoryContext context, CancellationToken ct = default)
    {
        if (context.Payload.TryGetValue("text", out var value) && value is string text)
            context.Payload["text"] = text.ToUpperInvariant();
        return Task.FromResult(TaskStepResult.Success());
    }
}

// With options
public class MyPipelineOptions { public int MaxLength { get; set; } = 1000; }

public class MyOptionsPipeline : IMemoryPipeline<MyPipelineOptions>
{
    public Task<TaskStepResult> ExecuteAsync(MemoryContext context, MyPipelineOptions options, CancellationToken ct = default)
    {
        if (context.Payload.TryGetValue("text", out var value) && value is string text && text.Length > options.MaxLength)
            context.Payload["text"] = text[..options.MaxLength];
        return Task.FromResult(TaskStepResult.Success());
    }
}

// Register
builder
    .Then<MyFilterPipeline>("filter")
    .Then<MyOptionsPipeline, MyPipelineOptions>("trim", new MyPipelineOptions { MaxLength = 500 });
```

## RAG + Agent Pattern

```csharp
// 1. Search relevant documents
var collection = await hive.Memory.GetCollectionAsync("qdrant", "docs");
var search     = await collection.SemanticSearchAsync("user question");

// 2. Build context string
var context = string.Join("\n\n", search.Results.Select(h =>
    $"[Source: {h.SourceId}]\n{(h.Payload.TryGetValue("text", out var t) ? t : null)}"));

// 3. Pass context to agent
var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider     = "openai";
    cfg.Model        = "gpt-4o";
    cfg.Instructions = $"Answer based on this context:\n\n{context}";
});

var response = await agent.InvokeAsync("user question");
```
