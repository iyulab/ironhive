# Storage Backends

IronHive has three storage abstractions: **IFileStorage**, **IVectorStorage**, and **IQueueStorage**.  
All are registered by name on `HiveServiceBuilder` and referenced by that name at runtime.

## IFileStorage (File Storage)

```csharp
public interface IFileStorage
{
    Task UploadAsync(string path, Stream data, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string path, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<IEnumerable<string>> ListAsync(string? prefix = null, CancellationToken ct = default);
}
```

Accessed via `hive.Files`:

```csharp
await hive.Files.UploadAsync("my-storage", "path/to/file.pdf", stream);
var s = await hive.Files.DownloadAsync("my-storage", "path/to/file.pdf");
```

### Local File Storage

```csharp
.AddLocalFileStorage("local", new LocalFileConfig { Path = "./storage" })
```

### Amazon S3

```csharp
// dotnet add package IronHive.Storages.Amazon
.AddAmazonS3Storage("s3", new AmazonS3Config
{
    BucketName      = "my-bucket",
    Region          = "us-east-1",
    AccessKeyId     = "AKIA...",
    SecretAccessKey = "..."
})
```

### Azure Blob Storage

```csharp
// dotnet add package IronHive.Storages.Azure
.AddAzureBlobStorage("azure-blob", new AzureBlobConfig
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;",
    ContainerName    = "hive-files"
})
```

### Azure File Share

```csharp
.AddAzureFilesStorage("azure-files", new AzureFilesConfig
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;",
    ShareName        = "hive-share"
})
```

---

## IVectorStorage (Vector DB)

```csharp
public interface IVectorStorage
{
    Task CreateCollectionAsync(string name, int dimensions, CancellationToken ct = default);
    Task DeleteCollectionAsync(string name, CancellationToken ct = default);
    Task<bool> CollectionExistsAsync(string name, CancellationToken ct = default);
    Task<IEnumerable<string>> ListCollectionsAsync(string? prefix = null, CancellationToken ct = default);

    Task UpsertAsync(string collection, IEnumerable<VectorRecord> records, CancellationToken ct = default);
    Task DeleteBySourceAsync(string collection, string sourceId, CancellationToken ct = default);
    Task<VectorSearchResult> SearchAsync(string collection, ReadOnlyMemory<float> vector, SearchOptions? options = null, CancellationToken ct = default);
}
```

Used internally by `IMemoryService`. Access via `hive.Memory`.

### Local Vector Storage (SQLite + sqlite-vec)

```csharp
.AddLocalVectorStorage("local-vec", new LocalVectorConfig { Path = "./vectors" })
```

### Qdrant

```csharp
// dotnet add package IronHive.Storages.Qdrant
.AddQdrantVectorStorage("qdrant", new QdrantConfig
{
    Endpoint = "http://localhost:6333",
    ApiKey   = "..."                    // optional
})
```

---

## IQueueStorage (Task Queue)

```csharp
public interface IQueueStorage
{
    Task EnqueueAsync<T>(string queueName, T item, CancellationToken ct = default);
    Task<T?> DequeueAsync<T>(string queueName, CancellationToken ct = default);
    Task<int> CountAsync(string queueName, CancellationToken ct = default);
    Task AckAsync(string queueName, string messageId, CancellationToken ct = default);
    Task NackAsync(string queueName, string messageId, CancellationToken ct = default);
}
```

Used internally by `MemoryWorker` pipeline. Specify name in `UseQueue("name")`.

### Local Queue Storage (file-based: .qmsg / .qlock / .qdead)

```csharp
.AddLocalQueueStorage("local-queue", new LocalQueueConfig { Path = "./queues" })
```

### RabbitMQ

```csharp
// dotnet add package IronHive.Storages.RabbitMQ
.AddRabbitMQQueueStorage("rabbit", new RabbitMQConfig
{
    ConnectionString = "amqp://user:pass@localhost:5672",
    VirtualHost      = "/"
})
```

---

## Common Setup Patterns

### Full Local Stack (no external dependencies)

```csharp
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." })
    .AddLocalFileStorage("files", new LocalFileConfig { Path = "./data/files" })
    .AddLocalVectorStorage("vectors", new LocalVectorConfig { Path = "./data/vectors" })
    .AddLocalQueueStorage("queue", new LocalQueueConfig { Path = "./data/queue" })
    .Build();
```

### Production Stack (S3 + Qdrant + RabbitMQ)

```csharp
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", config)
    .AddAmazonS3Storage("s3", s3Config)
    .AddQdrantVectorStorage("qdrant", qdrantConfig)
    .AddRabbitMQQueueStorage("rabbit", rabbitConfig)
    .Build();

// Create RAG collection using cloud stack
await hive.Memory.CreateCollectionAsync("qdrant", "documents", "openai", "text-embedding-3-small");

// MemoryWorker reads from RabbitMQ, writes to Qdrant
var worker = hive.CreateMemoryWorkerFrom(b =>
    b.UseQueue("rabbit")
     .Then<TextExtractionPipeline>("extract")
     .Then<TextChunkingPipeline, TextChunkingOptions>("chunk",
         new TextChunkingOptions { ChunkSize = 512, ChunkOverlap = 50 })
     .Then<CreateVectorsPipeline>("embed")
     .Then<StoreVectorsPipeline>("store")
     .Build());
```
