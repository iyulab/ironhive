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

// File source
await collection.IndexSourceAsync("local-queue", new FileMemorySource
{
    SourceId = "doc-001",
    FilePath = "./documents/manual.pdf"
});

// Text source
await collection.IndexSourceAsync("local-queue", new TextMemorySource
{
    SourceId = "text-001",
    Text     = "IronHive is a .NET 10 AI framework.",
    Metadata = new Dictionary<string, object>
    {
        ["category"] = "introduction",
        ["author"]   = "iyulab"
    }
});

// Remove a source
await collection.DeindexSourceAsync("doc-001");
```

## MemoryWorker

```csharp
var worker = hive.CreateMemoryWorkerFrom(builder =>
    builder
        .UseQueue("local-queue")                               // IQueueStorage name
        .Then<TextExtractionPipeline>("extract")               // extract text from file
        .Then<TextChunkingPipeline, TextChunkingOptions>("chunk",
            new TextChunkingOptions
            {
                ChunkSize    = 512,    // token-based chunk size
                ChunkOverlap = 50      // overlap between chunks
            })
        .Then<CreateVectorsPipeline>("embed")                  // generate embeddings
        .Then<StoreVectorsPipeline>("store")                   // save to vector storage
        .Build());

// Start background processing
await worker.StartAsync(cancellationToken);

// Monitor progress
worker.Progressed += (_, args) =>
{
    Console.WriteLine($"[{args.PipelineName}] {args.Status}: {args.Context.SourceId}");
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
             .Then<TextChunkingPipeline, TextChunkingOptions>("chunk",
                 new TextChunkingOptions { ChunkSize = 512, ChunkOverlap = 50 })
             .Then<CreateVectorsPipeline>("embed")
             .Then<StoreVectorsPipeline>("store")
             .Build());

        await worker.StartAsync(stoppingToken);
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
        TopK     = 10,      // number of results (default: 5)
        MinScore = 0.7f     // minimum similarity score 0–1
    });

foreach (var hit in results.Hits)
{
    Console.WriteLine($"[{hit.Score:F3}] {hit.Record.Content}");
    Console.WriteLine($"  Source: {hit.Record.Metadata?["source"]}");
}
```

## Built-in Pipelines

| Pipeline | Description |
|----------|-------------|
| `TextExtractionPipeline` | Extract text from PDF, DOCX, XLSX, PPTX, images, plain text |
| `TextChunkingPipeline` | Split text into semantic chunks (paragraph→sentence→clause→word) |
| `CreateVectorsPipeline` | Generate embeddings via configured provider |
| `StoreVectorsPipeline` | Store vectors in IVectorStorage |
| `DialogueExtractionPipeline` | Extract dialogue-formatted text |

## Custom Pipelines

```csharp
// Without options
public class MyFilterPipeline : IMemoryPipeline
{
    public Task<MemoryContext> ExecuteAsync(MemoryContext context, CancellationToken ct)
    {
        context.Text = context.Text?.ToUpper();
        return Task.FromResult(context);
    }
}

// With options
public class MyPipelineOptions { public int MaxLength { get; set; } = 1000; }

public class MyOptionsPipeline : IMemoryPipeline<MyPipelineOptions>
{
    public Task<MemoryContext> ExecuteAsync(MemoryContext context, MyPipelineOptions options, CancellationToken ct)
    {
        if (context.Text?.Length > options.MaxLength)
            context.Text = context.Text[..options.MaxLength];
        return Task.FromResult(context);
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
var context = string.Join("\n\n", search.Hits.Select(h =>
    $"[Source: {h.Record.Metadata?["source"]}]\n{h.Record.Content}"));

// 3. Pass context to agent
var agent = hive.CreateAgentFrom(cfg =>
{
    cfg.Provider     = "openai";
    cfg.Model        = "gpt-4o";
    cfg.Instructions = $"Answer based on this context:\n\n{context}";
});

var response = await agent.InvokeAsync("user question");
```
