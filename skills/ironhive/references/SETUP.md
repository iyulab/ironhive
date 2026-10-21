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
```

```csharp
// Inject IHiveService anywhere
public class MyService(IHiveService hive)
{
    public IAgent CreateAssistant() => hive.CreateAgentFrom(cfg => { cfg.Provider = "openai"; cfg.Model = "gpt-4o-mini"; });
}
```

## Provider Extension Methods

```csharp
using Google.Apis.Auth.OAuth2;   // GoogleCredential, for Vertex AI below

builder
// OpenAI — Chat, Embeddings, Images, Audio (TTS/STT)
.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKey       = "sk-...",
    Organization = "org-...",       // optional
    BaseUrl      = "https://.../v1" // optional override — full endpoint incl. version segment
})

// Anthropic — Chat only
.AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = "sk-ant-..." })

// Google AI — Chat, Embeddings, Images (Imagen), Video (Veo), Audio
.AddGoogleAIProviders("google", new GoogleAIConfig { ApiKey = "AIza..." })

// Vertex AI — same services via GCP
.AddVertexAIProviders("vertex", new VertexAIConfig
{
    Project    = "my-project",
    Location   = "us-central1",
    Credential = GoogleCredential.GetApplicationDefault()   // optional: left null, the SDK uses ADC itself
})

// OpenAI-compatible (Ollama, LM Studio, vLLM, DeepSeek, Groq…)
.AddOpenAICompatibleProviders("ollama", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:11434",   // "/v1" (Path) is appended
    ApiKey  = "ollama"                    // optional
})

// GPUStack shortcut
.AddGpuStackProviders("gpustack", new GpuStackConfig
{
    BaseUrl = "http://localhost:80",
    ApiKey  = "your-key"
});
```

## File Storage

```csharp
builder
// Amazon S3
.AddAmazonS3Storage("s3", new AmazonS3Config
{
    BucketName      = "my-bucket",
    RegionCode      = "us-east-1",
    AccessKey       = "...",
    SecretAccessKey = "..."
})

// Azure Blob — StorageName is the container name
.AddAzureBlobStorage("azure-blob", new AzureStorageConfig
{
    ConnectionString = "DefaultEndpointsProtocol=...",
    StorageName      = "hive-files"
})

// Azure File Share — StorageName is the share name
.AddAzureFilesStorage("azure-files", new AzureStorageConfig
{
    ConnectionString = "...",
    StorageName      = "hive-share"
});
```

## Vector and Queue Storage

Not registered on the builder — construct the storage and use the instance directly.

```csharp
// Qdrant vector DB (gRPC, default port 6334)
using var vectors = new QdrantVectorStorage(new QdrantConfig { Host = "localhost", Port = 6334 });

// RabbitMQ queue — one IQueueStorage per queue
using var queue = new RabbitMQueueStorage(new RabbitMQConfig
{
    Host      = "localhost",
    UserName  = "guest",
    Password  = "guest",
    QueueName = "hive-tasks"
});
```

## HiveServiceBuilder Full Signature (key methods)

```csharp
public class HiveServiceBuilder
{
    // Providers
    // (extension methods from the provider packages)
    IHiveServiceBuilder AddOpenAIProviders(string name, OpenAIConfig config, OpenAIServiceType serviceType = OpenAIServiceType.All);
    IHiveServiceBuilder AddAnthropicProviders(string name, AnthropicConfig config, AnthropicServiceType serviceType = AnthropicServiceType.All);
    IHiveServiceBuilder AddGoogleAIProviders(string name, GoogleAIConfig config, GoogleAIServiceType serviceType = GoogleAIServiceType.All);
    IHiveServiceBuilder AddVertexAIProviders(string name, VertexAIConfig config, GoogleAIServiceType serviceType = GoogleAIServiceType.All);
    IHiveServiceBuilder AddOpenAICompatibleProviders(string name, OpenAICompatibleConfig config, OpenAICompatibleServiceType serviceType = OpenAICompatibleServiceType.All);
    IHiveServiceBuilder AddGpuStackProviders(string name, GpuStackConfig config, GpuStackServiceType serviceType = GpuStackServiceType.All);

    // File storage (storage package extensions: AddAmazonS3Storage, AddAzureBlobStorage, AddAzureFilesStorage)
    // Any file storage instance (IHiveServiceBuilder member)
    IHiveServiceBuilder AddFileStorage(string name, IFileStorage storage);

    // Build
    IHiveService Build();
}
```
