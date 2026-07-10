# 핵심 서비스

`IHiveService`가 제공하는 각 서비스의 상세 설명입니다.

## IHiveService

```csharp
public interface IHiveService : IDisposable
{
    IModelService Models { get; }
    IMessageService Messages { get; }
    IEmbeddingService Embeddings { get; }
    IImageService Images { get; }
    IVideoService Videos { get; }
    IAudioService Audio { get; }
    IFileStorageService Files { get; }
    IMemoryService Memory { get; }

    IAgent CreateAgentFrom(Action<AgentConfig> configure);
    IAgent CreateAgentFrom(AgentCard card);
    IAgent CreateAgentFromYaml(string yaml);
}
```

---

## IMessageService

LLM과의 대화를 처리하고 도구 호출 루프를 관리합니다.

```csharp
public interface IMessageService
{
    Task<MessageResponse> GenerateMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default);

    Task<int> CountTokensAsync(
        MessageRequest request,
        CancellationToken cancellationToken = default);
}
```

### MessageRequest

```csharp
var request = new MessageRequest
{
    Provider = "openai",
    Model = "gpt-4o-mini",
    System = "You are helpful.",
    Messages = messages,
    Tools = toolCollection,
    ThinkingEffort = MessageThinkingEffort.High,  // 추론 모드
    PreviousId = "prev-response-id"               // 대화 연속성 (Responses API)
};
```

### 도구 실행 루프

`MessageService`는 자동으로 도구 호출을 처리합니다:

1. LLM이 `tool_use` 응답 생성
2. `IToolCollection`에서 도구 찾아 실행 (최대 3개 병렬)
3. 결과를 `ToolMessage`로 추가
4. LLM에 재전송
5. `stop` 응답까지 반복

도구 실행 자체를 가로채고 싶다면(입력 조정, 결과 변환, mock 등) `MessageRequest.ToolOptions.OnBeforeInvoke`/`OnAfterInvoke`를 사용하세요 — [TOOLS.md](TOOLS.md) 참조.

### 메시지 생성 미들웨어 (IMessageMiddleware)

`generator` 호출(각 루프 반복의 실제 LLM 호출) 전후를 감싸는 next-체인 미들웨어입니다. compaction, 재시도, 에러 핸들링처럼 "루프를 도는 동안 매 라운드 개입해야 하는" 관심사에 씁니다.

```csharp
public interface IMessageMiddleware
{
    Task<MessageResponse> GenerateAsync(
        MessageGenerationRequest request,
        Func<MessageGenerationRequest, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingAsync(
        MessageGenerationRequest request,
        Func<MessageGenerationRequest, IAsyncEnumerable<StreamingMessageResponse>> next,
        CancellationToken cancellationToken = default);
}
```

둘 다 기본 구현(pass-through)이 있어 원하는 쪽만 override하면 됩니다. `HiveServiceBuilder.AddMessageMiddleware()`로 전역 등록하며, 등록 순서대로 바깥쪽부터 감쌉니다.

```csharp
public class CompactingMiddleware : IMessageMiddleware
{
    public async Task<MessageResponse> GenerateAsync(
        MessageGenerationRequest request,
        Func<MessageGenerationRequest, Task<MessageResponse>> next,
        CancellationToken cancellationToken = default)
    {
        if (request.Messages.Count > 50)
        {
            // request.Messages를 요약본으로 교체하는 등 next() 호출 전에 손볼 수 있습니다.
        }

        return await next(request);
    }
}

var hive = new HiveServiceBuilder()
    .AddOpenAIProviders("openai", config)
    .AddMessageMiddleware(new CompactingMiddleware())
    .Build();
```

`next()`를 try/catch로 감싸면 재시도·에러 대체 응답도 표현할 수 있습니다(`IAgentMiddleware`와 동일한 관용구, 한 레이어 아래인 `MessageService`에 적용된다는 차이만 있습니다 — [MIDDLEWARE.md](MIDDLEWARE.md) 참조).

### 컨텍스트 윈도우 초과 예외

프로바이더(OpenAI, Anthropic, GoogleAI, OpenAI Compatible)별로 다른 컨텍스트 윈도우 초과 오류 형식을 `ContextOverflowException`(`IronHive.Abstractions.Exceptions`)으로 정규화합니다. 문자열 파싱 없이 `catch`로 압축/재시도 로직을 작성할 수 있습니다.

```csharp
try
{
    var response = await hive.Messages.GenerateMessageAsync(request);
}
catch (ContextOverflowException ex)
{
    // ex.ContextWindow — 모델 컨텍스트 윈도우 크기 (프로바이더가 보고하지 않으면 null)
    // 오래된 메시지를 요약해 request.Messages를 줄이고 재시도하는 등의 복구 로직
}
```

---

## IEmbeddingService

텍스트를 벡터로 변환합니다.

```csharp
public interface IEmbeddingService
{
    // 단일 입력 임베딩 → float[] 반환
    Task<float[]> EmbedAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default);

    // 배치 임베딩 → (벡터 + 토큰 수) 목록 반환
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    // 단일 입력 토큰 수 계산
    Task<int> CountTokensAsync(
        string provider,
        string modelId,
        string input,
        CancellationToken cancellationToken = default);

    // 배치 토큰 수 계산
    Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string provider,
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);
}

// 사용 예시
float[] vector = await hive.Embeddings.EmbedAsync("openai", "text-embedding-3-small", "텍스트");

var batch = await hive.Embeddings.EmbedBatchAsync("openai", "text-embedding-3-small", texts);
foreach (var r in batch)
    Console.WriteLine(r.Vector.Length); // 임베딩 차원
```

---

## IImageService

이미지를 생성하거나 편집합니다.

```csharp
public interface IImageService
{
    Task<ImageGenerationResponse> GenerateAsync(
        ImageGenerationRequest request,
        CancellationToken cancellationToken = default);

    Task<ImageGenerationResponse> EditAsync(
        ImageEditRequest request,
        CancellationToken cancellationToken = default);
}

// 사용 예시
var response = await hive.Images.GenerateAsync(new ImageGenerationRequest
{
    Provider = "openai",
    Model = "dall-e-3",
    Prompt = "A futuristic city at sunset",
    Size = GeneratedImageSize.Square1024
});

foreach (var image in response.Images)
{
    Console.WriteLine(image.Url ?? Convert.ToBase64String(image.Data ?? []));
}
```

---

## IVideoService

비디오를 생성합니다 (비동기 폴링 방식).

```csharp
public interface IVideoService
{
    Task<VideoGenerationResponse> GenerateAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);
}

// 사용 예시 (Google Veo 등)
var response = await hive.Videos.GenerateAsync(new VideoGenerationRequest
{
    Provider = "google",
    Model = "veo-2.0-generate-001",
    Prompt = "A serene mountain lake at dawn",
    Size = GeneratedVideoSize.Landscape720p
});

foreach (var video in response.Videos)
{
    Console.WriteLine(video.Url);
}
```

---

## IAudioService

Text-to-Speech 및 Speech-to-Text를 처리합니다.

```csharp
public interface IAudioService
{
    Task<TextToSpeechResponse> TextToSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);

    Task<SpeechToTextResponse> SpeechToTextAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default);
}

// TTS
var ttsResponse = await hive.Audio.TextToSpeechAsync(new TextToSpeechRequest
{
    Provider = "openai",
    Model = "tts-1",
    Voice = "alloy",
    Text = "안녕하세요, IronHive입니다."
});
var audioBytes = ttsResponse.Audio.Data;

// STT
var sttResponse = await hive.Audio.SpeechToTextAsync(new SpeechToTextRequest
{
    Provider = "openai",
    Model = "whisper-1",
    AudioData = audioBytes,
    Language = "ko"
});
Console.WriteLine(sttResponse.Text);
```

---

## IModelService

등록된 프로바이더의 모델 목록을 조회합니다.

```csharp
public interface IModelService
{
    Task<ModelCardList> ListModelsAsync(
        string provider,
        CancellationToken cancellationToken = default);

    Task<IModelCard?> FindModelAsync(
        string provider,
        string model,
        CancellationToken cancellationToken = default);
}

// 사용 예시
var models = await hive.Models.ListModelsAsync("openai");
foreach (var model in models)
{
    Console.WriteLine($"{model.Id} — {model.Description}");
}
```

---

## IFileStorageService

등록된 스토리지 백엔드로 파일을 업로드/다운로드합니다.

```csharp
public interface IFileStorageService
{
    Task UploadAsync(string storageName, string path, Stream data, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string storageName, string path, CancellationToken ct = default);
    Task DeleteAsync(string storageName, string path, CancellationToken ct = default);
    Task<IEnumerable<string>> ListAsync(string storageName, string? prefix = null, CancellationToken ct = default);
}

// 사용 예시
await hive.Files.UploadAsync("s3", "documents/report.pdf", pdfStream);
var stream = await hive.Files.DownloadAsync("s3", "documents/report.pdf");
```

---

## IMemoryService

벡터 컬렉션 관리 및 RAG 서비스입니다.

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

### IMemoryCollection

```csharp
public interface IMemoryCollection
{
    string StorageName { get; }
    string CollectionName { get; }
    string EmbeddingProvider { get; }
    string EmbeddingModel { get; }

    // 소스 인덱싱 (큐에 작업 등록)
    Task IndexSourceAsync(string queueName, IMemorySource source, CancellationToken ct = default);

    // 소스 제거
    Task DeindexSourceAsync(string sourceId, CancellationToken ct = default);

    // 의미적 유사도 검색
    Task<VectorSearchResult> SemanticSearchAsync(string query, SearchOptions? options = null, CancellationToken ct = default);
}
```

### RAG 검색 예시

```csharp
// 컬렉션 생성
await hive.Memory.CreateCollectionAsync("qdrant", "documents", "openai", "text-embedding-3-small");

// 컬렉션 조회 및 검색
var collection = await hive.Memory.GetCollectionAsync("qdrant", "documents");
var results = await collection.SemanticSearchAsync("인공지능의 역사", new SearchOptions
{
    TopK = 5,
    MinScore = 0.7f
});

foreach (var hit in results.Hits)
{
    Console.WriteLine($"[{hit.Score:F2}] {hit.Record.Content}");
}
```

자세한 RAG 파이프라인은 [MEMORY.md](MEMORY.md)를 참조하세요.

---

## MemoryWorker (CreateMemoryWorkerFrom)

`HiveService`의 확장 메서드로 RAG 인제스션 워커를 생성합니다:

```csharp
var worker = hive.CreateMemoryWorkerFrom(builder =>
    builder
        .UseQueue("local-queue")
        .Then<TextExtractionPipeline>("extract")
        .Then<TextChunkingPipeline, TextChunkingOptions>("chunk", new TextChunkingOptions
        {
            ChunkSize = 512,
            ChunkOverlap = 50
        })
        .Then<CreateVectorsPipeline>("embed")
        .Then<StoreVectorsPipeline>("store")
        .Build());

await worker.StartAsync(cancellationToken);
```

자세한 내용은 [MEMORY.md](MEMORY.md)를 참조하세요.

---

## IFileParserService (별도 DI 등록)

`AddFileParser()`로 등록합니다. `IHiveService`와 독립적입니다.

```csharp
// DI 등록 (Program.cs)
builder.Services.AddFileParser();

// 사용
public class ParseService(IFileParserService parser)
{
    public async Task<string> ExtractTextAsync(string fileName, Stream data)
    {
        var blocks = await parser.ParseAsync(fileName, data);
        return string.Join("\n", blocks.Select(b => b.Text));
    }
}
```

지원 파일: PDF, DOCX, XLSX, PPTX, PNG/JPG 등 이미지, 기타 텍스트 파일

---

## 메시지 타입

### Message

```csharp
public class Message
{
    public MessageRole Role { get; set; }          // User / Assistant
    public List<MessageContent> Content { get; set; }
}

public enum MessageRole { User, Assistant }
```

### MessageContent 타입 (폴리모픽)

```csharp
// 타입 식별자: "text", "image", "tool", "thinking"
TextMessageContent { Value: string }
ImageMessageContent { MediaType: string, Data: string (base64) | Url: string }
ToolMessageContent  { /* 도구 호출/결과 */ }
ThinkingMessageContent { Thinking: string }  // Anthropic Extended Thinking
```

### 메시지 구성 예시

```csharp
var messages = new List<Message>
{
    new Message
    {
        Role = MessageRole.User,
        Content =
        [
            new TextMessageContent { Value = "이 이미지를 설명해주세요." },
            new ImageMessageContent
            {
                MediaType = "image/png",
                Data = Convert.ToBase64String(imageBytes)
            }
        ]
    }
};
```

---

## 관련 문서

- [MEMORY.md](MEMORY.md) — RAG 파이프라인
- [TOOLS.md](TOOLS.md) — 도구 시스템
- [AGENTS.md](AGENTS.md) — 에이전트
