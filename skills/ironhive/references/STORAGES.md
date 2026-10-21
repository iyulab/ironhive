# Storage Backends

IronHive has three storage abstractions: **IFileStorage**, **IVectorStorage**, and **IQueueStorage**.
File storages are registered by name on `HiveServiceBuilder` (`AddFileStorage`, or a package's shortcut extension) and
reached through `hive.Files` by that name. Vector and queue storages are not registered on the builder — construct the
storage and call the instance directly. For document ingestion and hybrid search use FluxIndex.

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

### Amazon S3

```csharp
// dotnet add package IronHive.Storages.Amazon
builder
.AddAmazonS3Storage("s3", new AmazonS3Config
{
    BucketName      = "my-bucket",
    RegionCode      = "us-east-1",
    AccessKey       = "AKIA...",
    SecretAccessKey = "..."
});
```

### Azure Blob Storage

```csharp
// dotnet add package IronHive.Storages.Azure
builder
.AddAzureBlobStorage("azure-blob", new AzureStorageConfig
{
    AuthType         = AzureStorageAuthTypes.ConnectionString,   // default; also AccountKey | SASToken | AzureIdentity
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;",
    StorageName      = "hive-files"                              // container name
});
```

### Azure File Share

```csharp
builder
.AddAzureFilesStorage("azure-files", new AzureStorageConfig
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;",
    StorageName      = "hive-share"                              // file share name
});
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

### Qdrant

```csharp
// dotnet add package IronHive.Storages.Qdrant — construct the storage and use it directly
using var vectors = new QdrantVectorStorage(new QdrantConfig
{
    Host   = "localhost",
    Port   = 6334,                 // gRPC port (default)
    Https  = false,
    ApiKey = "..."                 // optional
});
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

### RabbitMQ

```csharp
// dotnet add package IronHive.Storages.RabbitMQ — construct the storage and use it directly
using var queue = new RabbitMQueueStorage(new RabbitMQConfig
{
    Host        = "localhost",
    Port        = 5672,
    UserName    = "user",
    Password    = "pass",
    VirtualHost = "/",
    QueueName   = "hive-tasks"     // required
});
```

---

## Common Setup Pattern

### Providers + file storage, vector and queue storages alongside

```csharp
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", config)
    .AddAmazonS3Storage("s3", s3Config)
    .Build();

using var vectors = new QdrantVectorStorage(qdrantConfig);
using var queue = new RabbitMQueueStorage(rabbitConfig);
```
