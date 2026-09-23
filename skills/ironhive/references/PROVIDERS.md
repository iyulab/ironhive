# AI Provider Configurations

All providers are registered on `HiveServiceBuilder` by name. The name string is how you reference the provider later (in `AgentConfig.Provider`, `MessageRequest.Provider`, etc.).

## OpenAI

```csharp
.AddOpenAIProviders("openai", new OpenAIConfig
{
    ApiKey       = "sk-...",
    Organization = "org-...",       // optional
    Project      = "proj_...",      // optional
    BaseUrl      = "https://.../v1" // optional — full endpoint incl. the version segment (proxy/gateway)
}, OpenAIServiceType.All)           // optional flag — see below
```

### OpenAIServiceType Flags

```csharp
[Flags]
public enum OpenAIServiceType
{
    Models      = 1 << 0,   // Model listing
    Messages    = 1 << 1,   // Chat
    Embeddings  = 1 << 2,   // Embeddings
    Images      = 1 << 3,   // Image generation
    Audio       = 1 << 4,   // TTS + STT
    All         = Models | Messages | Embeddings | Images | Audio
}
```

**Capabilities:** Chat, Embeddings, Images (DALL-E), TTS, STT, Model listing

## Anthropic

```csharp
.AddAnthropicProviders("anthropic", new AnthropicConfig
{
    ApiKey = "sk-ant-...",
    BaseUrl = "https://..."   // optional proxy
})
```

**Capabilities:** Chat only (Claude models)  
**Special:** Extended thinking (`ThinkingEffort.High`)

## Google AI (Gemini)

```csharp
.AddGoogleAIProviders("google", new GoogleAIConfig
{
    ApiKey = "AIzaSy..."
})
```

**Capabilities:** Chat, Embeddings, Images (Imagen), Video (Veo), Audio, Model listing

## Vertex AI (GCP)

```csharp
.AddVertexAIProviders("vertex", new VertexAIConfig
{
    Project    = "my-gcp-project",
    Location   = "us-central1",
    Credential = GoogleCredential.GetApplicationDefault()   // required (Google.Apis.Auth ICredential)
})
```

**Capabilities:** Same as Google AI but via GCP project billing

## OpenAI Compatible

Supports any provider with OpenAI-compatible API:

```csharp
.AddOpenAICompatibleProviders("provider-name", new OpenAICompatibleConfig
{
    BaseUrl = "http://localhost:11434",   // server address without the API path; Path (default "/v1") is appended
    ApiKey  = "ollama"                    // optional — LAN services often accept no key
})
```

**Known compatible providers:** Ollama, LM Studio, vLLM, DeepSeek, Groq, OpenRouter, Fireworks, Perplexity, TogetherAI, xAI (Grok)

## GPUStack

```csharp
.AddGpuStackProviders("gpustack", new GpuStackConfig
{
    BaseUrl = "http://localhost:80",
    ApiKey  = "your-api-key"
})
```

**Capabilities:** Chat, Embeddings (model-dependent)

## Provider Name Usage

Once registered, use the name everywhere:

```csharp
// In AgentConfig
cfg.Provider = "openai";
cfg.Model    = "gpt-4o-mini";

// In MessageRequest
var req = new MessageRequest
{
    Provider = "anthropic",
    Model    = "claude-opus-4-5-20251001"
};

// In EmbeddingService
float[] emb = await hive.Embeddings.EmbedAsync("google", "text-embedding-004", text);

// In Memory collection
await hive.Memory.CreateCollectionAsync("qdrant", "docs", "openai", "text-embedding-3-small");
```

## Multiple Providers

```csharp
var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." })
    .AddAnthropicProviders("anthropic", new AnthropicConfig { ApiKey = "sk-ant-..." })
    .AddGoogleAIProviders("google", new GoogleAIConfig { ApiKey = "AIza..." })
    .AddOpenAICompatibleProviders("ollama", new OpenAICompatibleConfig
    {
        BaseUrl = "http://localhost:11434/v1",
        ApiKey  = "ollama"
    })
    .Build();

// Use any provider by name
var gpt4Agent    = hive.CreateAgentFrom(cfg => { cfg.Provider = "openai"; cfg.Model = "gpt-4o"; });
var claudeAgent  = hive.CreateAgentFrom(cfg => { cfg.Provider = "anthropic"; cfg.Model = "claude-opus-4-8"; });
var geminiAgent  = hive.CreateAgentFrom(cfg => { cfg.Provider = "google"; cfg.Model = "gemini-2.0-flash"; });
var ollamaAgent  = hive.CreateAgentFrom(cfg => { cfg.Provider = "ollama"; cfg.Model = "llama3.2"; });
```
