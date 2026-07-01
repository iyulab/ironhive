# Setup & Registration

## Installation

```bash
dotnet add package IronHive.Core
dotnet add package IronHive.Providers.OpenAI       # Chat, Embeddings, DALL-E, TTS/STT
dotnet add package IronHive.Providers.Anthropic    # Claude
dotnet add package IronHive.Providers.GoogleAI     # Gemini, Vertex AI, Veo, Imagen
dotnet add package IronHive.Providers.OpenAI.Compatible  # Ollama, LM Studio, vLLM, etc.
```

## Standalone (no DI)

```csharp
using IronHive.Core;
using IronHive.Providers.OpenAI;

var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." })
    .Build();
```

## ASP.NET Core DI

```csharp
// Program.cs
builder.Services.AddHiveService((hiveBuilder, sp) =>
    hiveBuilder
        .AddOpenAIProviders("openai", new OpenAIConfig
        {
            ApiKey = builder.Configuration["OpenAI:ApiKey"]!
        })
        .Build());

// Optional: file parsing service (independent of IHiveService)
builder.Services.AddFileParser();
```

```csharp
// Inject IHiveService anywhere
public class MyService(IHiveService hive) { ... }
```

## Provider Extension Methods

```csharp
// OpenAI — Chat, Embeddings, Images, Audio (TTS/STT)
.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKey  = "sk-...",
    OrgId   = "org-...",        // optional
    BaseUrl = "https://..."     // optional override
})

// Anthropic — Chat only
.AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = "sk-ant-..." })

// Google AI — Chat, Embeddings, Images (Imagen), Video (Veo), Audio
.AddGoogleAIProviders("google", new GoogleAIConfig { ApiKey = "AIza..." })

// Vertex AI — same services via GCP
.AddVertexAIProviders("vertex", new VertexAIConfig
{
    ProjectId = "my-project",
    Location  = "us-central1",
    Credentials = "path/to/sa.json"   // optional
})

// OpenAI-compatible (Ollama, LM Studio, vLLM, DeepSeek, Groq…)
.AddOpenAICompatibleProviders("ollama", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:11434/v1",
    ApiKey  = "ollama"
})

// GPUStack shortcut
.AddGpuStackProviders("gpustack", new GpuStackConfig
{
    BaseUrl = "http://localhost:80",
    ApiKey  = "your-key"
})
```

## Local Storage Extension Methods

```csharp
// File storage (local filesystem)
.AddLocalFileStorage("local-files", new LocalFileConfig { Path = "./storage" })

// Vector storage (SQLite + sqlite-vec)
.AddLocalVectorStorage("local-vec", new LocalVectorConfig { Path = "./vectors" })

// Queue storage (file-based .qmsg/.qlock/.qdead)
.AddLocalQueueStorage("local-queue", new LocalQueueConfig { Path = "./queues" })
```

## External Storage

```csharp
// Amazon S3
.AddAmazonS3Storage("s3", new AmazonS3Config
{
    BucketName      = "my-bucket",
    Region          = "us-east-1",
    AccessKeyId     = "...",
    SecretAccessKey = "..."
})

// Azure Blob
.AddAzureBlobStorage("azure-blob", new AzureBlobConfig
{
    ConnectionString = "DefaultEndpointsProtocol=...",
    ContainerName    = "hive-files"
})

// Azure File Share
.AddAzureFilesStorage("azure-files", new AzureFilesConfig
{
    ConnectionString = "...",
    ShareName        = "hive-share"
})

// Qdrant vector DB
.AddQdrantVectorStorage("qdrant", new QdrantConfig { Endpoint = "http://localhost:6333" })

// RabbitMQ queue
.AddRabbitMQQueueStorage("rabbit", new RabbitMQConfig { ConnectionString = "amqp://..." })
```

## HiveServiceBuilder Full Signature (key methods)

```csharp
public class HiveServiceBuilder
{
    // Providers
    IHiveServiceBuilder AddOpenAIProviders(string name, OpenAIConfig config, OpenAIServiceType types = All);
    IHiveServiceBuilder AddAnthropicProviders(string name, AnthropicConfig config);
    IHiveServiceBuilder AddGoogleAIProviders(string name, GoogleAIConfig config);
    IHiveServiceBuilder AddVertexAIProviders(string name, VertexAIConfig config);
    IHiveServiceBuilder AddOpenAICompatibleProviders(string name, OpenAICompatibleConfig config);
    IHiveServiceBuilder AddGpuStackProviders(string name, GpuStackConfig config);

    // Storage
    IHiveServiceBuilder AddLocalFileStorage(string name, LocalFileConfig config);
    IHiveServiceBuilder AddLocalVectorStorage(string name, LocalVectorConfig config);
    IHiveServiceBuilder AddLocalQueueStorage(string name, LocalQueueConfig config);

    // Build
    IHiveService Build();
}
```
