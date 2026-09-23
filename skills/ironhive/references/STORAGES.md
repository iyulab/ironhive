# Storage Backends

IronHive has three storage abstractions: **IFileStorage**, **IVectorStorage**, and **IQueueStorage**.  
All are registered by name on `HiveServiceBuilder` and referenced by that name at runtime.
Any implementation can be registered with `AddFileStorage` / `AddVectorStorage` / `AddQueueStorage`;
some packages add a shortcut extension on top.

## IFileStorage (File Storage)

```csharp
public interface IFileStorage : IDisposable
{
    Task<IEnumerable<string>> ListAsync(string? prefix = null, int depth = 1, CancellationToken ct = default);
    Task<bool> ExistsFileAsync(string filePath, CancellationToken ct = default);
    Task<Stream> ReadFileAsync(string filePath, CancellationToken ct = default);
    Task WriteFileAsync(string filePath, Stream data, bool overwrite = true, CancellationToken ct = default);
    Task DeleteFileAsync(string filePath, CancellationToken ct = default);
    Task DeleteDirectoryAsync(string directoryPath, CancellationToken ct = default);
}
```

Accessed via `hive.Files` (same methods with the storage name first):

```csharp
await hive.Files.WriteFileAsync("my-storage", "path/to/file.pdf", stream);
var s = await hive.Files.ReadFileAsync("my-storage", "path/to/file.pdf");
```

### Local File Storage

```csharp
.AddLocalFileStorage("local")    // no config — file paths are used as given
```

### Amazon S3

```csharp
// dotnet add package IronHive.Storages.Amazon
.AddAmazonS3Storage("s3", new AmazonS3Config
{
    BucketName      = "my-bucket",
    RegionCode      = "us-east-1",
    AccessKey       = "AKIA...",
    SecretAccessKey = "..."
})
```

### Azure Blob Storage

```csharp
// dotnet add package IronHive.Storages.Azure
.AddAzureBlobStorage("azure-blob", new AzureStorageConfig
{
    AuthType         = AzureStorageAuthTypes.ConnectionString,   // default; also AccountKey | SASToken | AzureIdentity
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;",
    StorageName      = "hive-files"                              // container name
})
```

### Azure File Share

```csharp
.AddAzureFilesStorage("azure-files", new AzureStorageConfig
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;",
    StorageName      = "hive-share"                              // file share name
})
```

---

## IVectorStorage (Vector DB)

```csharp
public interface IVectorStorage : IDisposable
{
    Task<IEnumerable<VectorCollectionInfo>> ListCollectionsAsync(CancellationToken ct = default);
    Task<bool> CollectionExistsAsync(string collectionName, CancellationToken ct = default);
    Task<VectorCollectionInfo?> GetCollectionInfoAsync(string collectionName, CancellationToken ct = default);
    Task CreateCollectionAsync(VectorCollectionInfo collection, CancellationToken ct = default);
    Task DeleteCollectionAsync(string collectionName, CancellationToken ct = default);

    Task<IEnumerable<VectorRecord>> FindVectorsAsync(string collectionName, int limit = 20, VectorRecordFilter? filter = null, CancellationToken ct = default);
    Task UpsertVectorsAsync(string collectionName, IEnumerable<VectorRecord> vectors, CancellationToken ct = default);
    Task DeleteVectorsAsync(string collectionName, VectorRecordFilter filter, CancellationToken ct = default);
    Task<IEnumerable<ScoredVectorRecord>> SearchVectorsAsync(string collectionName, float[] vector, float minScore = 0.0f, int limit = 5, VectorRecordFilter? filter = null, CancellationToken ct = default);
}
```

Used internally by `IMemoryService`. Access via `hive.Memory`.

### Local Vector Storage (SQLite + sqlite-vec)

```csharp
.AddLocalVectorStorage("local-vec", new LocalVectorConfig { DatabasePath = "./data/vectors.db" })
```

### Qdrant

```csharp
// dotnet add package IronHive.Storages.Qdrant — no shortcut extension; register the storage instance
.AddVectorStorage("qdrant", new QdrantVectorStorage(new QdrantConfig
{
    Host   = "localhost",
    Port   = 6334,                 // gRPC port (default)
    Https  = false,
    ApiKey = "..."                 // optional
}))
```

---

## IQueueStorage (Task Queue)

Each `IQueueStorage` instance is one queue.

```csharp
public interface IQueueStorage : IDisposable
{
    Task<IQueueConsumer> CreateConsumerAsync<T>(Func<IQueueMessage<T>, Task> onReceived, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
    Task EnqueueAsync<T>(T message, CancellationToken ct = default);
    Task<IQueueMessage<T>?> DequeueAsync<T>(CancellationToken ct = default);
}
```

Used internally by `MemoryWorker` pipeline. Specify name in `UseQueue("name")`.

### Local Queue Storage (file-based: .qmsg / .qlock / .qdead)

```csharp
.AddLocalQueueStorage("local-queue", new LocalQueueConfig { DirectoryPath = "./data/queue" })
```

### RabbitMQ

```csharp
// dotnet add package IronHive.Storages.RabbitMQ — no shortcut extension; register the storage instance
.AddQueueStorage("rabbit", new RabbitMQueueStorage(new RabbitMQConfig
{
    Host        = "localhost",
    Port        = 5672,
    UserName    = "user",
    Password    = "pass",
    VirtualHost = "/",
    QueueName   = "hive-tasks"     // required
}))
```

---

## Common Setup Patterns

### Full Local Stack (no external dependencies)

```csharp
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." })
    .AddLocalFileStorage("files")
    .AddLocalVectorStorage("vectors", new LocalVectorConfig { DatabasePath = "./data/vectors.db" })
    .AddLocalQueueStorage("queue", new LocalQueueConfig { DirectoryPath = "./data/queue" })
    .Build();
```

### Production Stack (S3 + Qdrant + RabbitMQ)

```csharp
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", config)
    .AddAmazonS3Storage("s3", s3Config)
    .AddVectorStorage("qdrant", new QdrantVectorStorage(qdrantConfig))
    .AddQueueStorage("rabbit", new RabbitMQueueStorage(rabbitConfig))
    .Build();

// Create RAG collection using cloud stack
await hive.Memory.CreateCollectionAsync("qdrant", "documents", "openai", "text-embedding-3-small");

// MemoryWorker reads from RabbitMQ, writes to Qdrant
var worker = hive.CreateMemoryWorkerFrom(b =>
    b.UseQueue("rabbit")
     .Then<TextExtractionPipeline>("extract")
     .Then<TextChunkingPipeline, TextChunkingPipeline.Options>("chunk",
         new TextChunkingPipeline.Options(ChunkSize: 512, ChunkOverlap: 50))
     .Then<CreateVectorsPipeline>("embed")
     .Then<StoreVectorsPipeline>("store")
     .Build());
```
