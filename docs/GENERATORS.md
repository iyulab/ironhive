# Generator Architecture Design

## 개요
IronHive의 Generator 아키텍처는 일관된 패턴으로 다양한 AI 생성 기능을 제공합니다. 
기존의 MessageGenerator와 EmbeddingGenerator 패턴을 확장하여 Image, Video, Audio 생성 기능을 추가합니다.

## 아키텍처 패턴

### 계층 구조
각 Generator는 3개의 계층으로 구성됩니다:

```
┌─────────────────────────────────────────┐
│   IronHive.Abstractions                  │
│   - IXxxGenerator (Provider Interface)   │
│   - IXxxService (Application Interface)  │
│   - Request/Response Models              │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   IronHive.Core                          │
│   - XxxService (Implementation)          │
│   - Extensions                           │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   IronHive.Providers.XXX                 │
│   - XXXImageGenerator                    │
│   - XXXVideoGenerator                    │
│   - XXXAudioGenerator                    │
└─────────────────────────────────────────┘
```

### 핵심 원칙
1. **Provider Interface (IXxxGenerator)**: 프로바이더 구현자를 위한 저수준 인터페이스
2. **Application Interface (IXxxService)**: 애플리케이션 개발자를 위한 고수준 인터페이스
3. **Request/Response Pattern**: 명확한 입력/출력 계약
4. **Provider Registry**: 다중 프로바이더를 통합 관리
5. **DI Integration**: Microsoft.Extensions.DependencyInjection 기반

## 기존 Generator 패턴

### MessageGenerator 패턴
```csharp
// Abstractions Layer
public interface IMessageGenerator : IProviderItem
{
    Task<MessageResponse> GenerateMessageAsync(MessageGenerationRequest request, ...);
    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(...);
}

public interface IMessageService
{
    Task<MessageResponse> GenerateMessageAsync(MessageRequest request, ...);
    IAsyncEnumerable<StreamingMessageResponse> GenerateStreamingMessageAsync(...);
}

// Core Layer
public class MessageService : IMessageService
{
    private readonly IProviderRegistry _providers;
    // Provider를 선택하여 Generator 호출
}

// Provider Layer (OpenAI)
public class OpenAIChatMessageGenerator : IMessageGenerator
{
    private readonly OpenAIChatCompletionClient _client;
    // OpenAI API 호출 구현
}
```

### EmbeddingGenerator 패턴
```csharp
// Abstractions Layer
public interface IEmbeddingGenerator : IProviderItem
{
    Task<float[]> EmbedAsync(string modelId, string input, ...);
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(...);
    Task<int> CountTokensAsync(...);
}

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string provider, string modelId, string input, ...);
    Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(...);
}

// Core Layer
public class EmbeddingService : IEmbeddingService
{
    private readonly IProviderRegistry _providers;
    // Provider를 선택하여 Generator 호출
}
```

## 새로운 Generator 설계

### 2. VideoGenerator

#### 파일 구조
```
src/IronHive.Abstractions/Video/
├── IVideoGenerator.cs
├── IVideoService.cs
├── VideoGenerationRequest.cs
├── VideoGenerationResponse.cs
├── VideoEditRequest.cs
├── VideoFormat.cs
├── VideoResolution.cs
└── VideoStyle.cs

src/IronHive.Core/Services/
├── VideoService.cs

src/IronHive.Providers.OpenAI/
├── OpenAIVideoGenerator.cs (Sora)
```

#### 인터페이스 정의
```csharp
namespace IronHive.Abstractions.Video;

/// <summary>
/// 비디오 생성 기능을 제공하는 프로바이더 인터페이스입니다.
/// </summary>
public interface IVideoGenerator : IProviderItem
{
    /// <summary>
    /// 텍스트 프롬프트로부터 비디오를 생성합니다.
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoAsync(
        VideoGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 이미지로부터 비디오를 생성합니다. (Image-to-Video)
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoFromImageAsync(
        VideoFromImageRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 비디오를 편집합니다.
    /// </summary>
    Task<VideoGenerationResponse> EditVideoAsync(
        VideoEditRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 비디오 생성 작업의 상태를 조회합니다.
    /// (비디오 생성은 일반적으로 비동기 작업)
    /// </summary>
    Task<VideoGenerationStatus> GetGenerationStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 비디오 생성 서비스를 제공하는 애플리케이션 인터페이스입니다.
/// </summary>
public interface IVideoService
{
    /// <summary>
    /// 텍스트로부터 비디오를 생성합니다.
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoAsync(
        string provider,
        string modelId,
        string prompt,
        VideoGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 이미지로부터 비디오를 생성합니다.
    /// </summary>
    Task<VideoGenerationResponse> GenerateVideoFromImageAsync(
        string provider,
        string modelId,
        byte[] image,
        string? prompt = null,
        VideoGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 비디오 생성 작업 상태를 조회합니다.
    /// </summary>
    Task<VideoGenerationStatus> GetGenerationStatusAsync(
        string provider,
        string jobId,
        CancellationToken cancellationToken = default);
}
```

#### Request/Response 모델
```csharp
/// <summary>
/// 비디오 생성 요청
/// </summary>
public class VideoGenerationRequest
{
    public required string Model { get; set; }
    public required string Prompt { get; set; }
    public string? NegativePrompt { get; set; }
    public VideoResolution Resolution { get; set; } = VideoResolution.HD720p;
    public VideoFormat Format { get; set; } = VideoFormat.Mp4;
    public int DurationSeconds { get; set; } = 5;
    public int Fps { get; set; } = 30;
    public VideoStyle? Style { get; set; }
    public int? Seed { get; set; }
    public double? GuidanceScale { get; set; }
}

/// <summary>
/// 비디오 생성 응답
/// </summary>
public class VideoGenerationResponse
{
    public string? JobId { get; set; }
    public VideoGenerationStatus Status { get; set; }
    public GeneratedVideo? Video { get; set; }
    public VideoMetadata? Metadata { get; set; }
}

public class GeneratedVideo
{
    public byte[]? Data { get; set; }
    public string? Url { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int DurationSeconds { get; set; }
    public VideoResolution Resolution { get; set; }
    public int Fps { get; set; }
}

public enum VideoGenerationStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum VideoFormat
{
    Mp4,
    Webm,
    Mov,
    Gif
}

public enum VideoResolution
{
    SD480p,    // 854x480
    HD720p,    // 1280x720
    HD1080p,   // 1920x1080
    UHD4K      // 3840x2160
}

public enum VideoStyle
{
    Realistic,
    Cinematic,
    Animated,
    Documentary
}
```

### 3. AudioProcessor

#### 파일 구조
```
src/IronHive.Abstractions/Audio/
├── IAudioProcessor.cs
├── IAudioService.cs
├── AudioGenerationRequest.cs
├── AudioGenerationResponse.cs
├── TextToSpeechRequest.cs
├── SpeechToTextRequest.cs
├── MusicGenerationRequest.cs
├── AudioFormat.cs
├── AudioQuality.cs
└── Voice.cs

src/IronHive.Core/Services/
├── AudioService.cs

src/IronHive.Providers.OpenAI/
├── OpenAIAudioProcessor.cs (TTS, Whisper)
```

#### 인터페이스 정의
```csharp
namespace IronHive.Abstractions.Audio;

/// <summary>
/// 오디오 처리 기능(생성, 변환, 인식)을 제공하는 프로바이더 인터페이스입니다.
/// </summary>
public interface IAudioProcessor : IProviderItem
{
    /// <summary>
    /// 텍스트를 음성으로 변환합니다. (TTS)
    /// </summary>
    Task<AudioGenerationResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 음성을 텍스트로 변환합니다. (STT)
    /// </summary>
    Task<SpeechToTextResponse> TranscribeAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 텍스트 프롬프트로부터 음악을 생성합니다.
    /// </summary>
    Task<AudioGenerationResponse> GenerateMusicAsync(
        MusicGenerationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 오디오 효과를 생성합니다.
    /// </summary>
    Task<AudioGenerationResponse> GenerateSoundEffectAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 오디오 생성 서비스를 제공하는 애플리케이션 인터페이스입니다.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// 텍스트를 음성으로 변환합니다.
    /// </summary>
    Task<AudioGenerationResponse> GenerateSpeechAsync(
        string provider,
        string modelId,
        string text,
        Voice voice,
        AudioGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 음성을 텍스트로 변환합니다.
    /// </summary>
    Task<SpeechToTextResponse> TranscribeAsync(
        string provider,
        string modelId,
        byte[] audio,
        string? language = null,
        TranscriptionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 음악을 생성합니다.
    /// </summary>
    Task<AudioGenerationResponse> GenerateMusicAsync(
        string provider,
        string modelId,
        string prompt,
        MusicGenerationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 오디오 효과음을 생성합니다.
    /// </summary>
    Task<AudioGenerationResponse> GenerateSoundEffectAsync(
        string provider,
        string modelId,
        string prompt,
        AudioGenerationOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

#### Request/Response 모델
```csharp
/// <summary>
/// 텍스트를 음성으로 변환 요청
/// </summary>
public class TextToSpeechRequest
{
    public required string Model { get; set; }
    public required string Text { get; set; }
    public required Voice Voice { get; set; }
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;
    public AudioQuality Quality { get; set; } = AudioQuality.Standard;
    public double Speed { get; set; } = 1.0;
    public double? Pitch { get; set; }
}

/// <summary>
/// 음성을 텍스트로 변환 요청
/// </summary>
public class SpeechToTextRequest
{
    public required string Model { get; set; }
    public required byte[] Audio { get; set; }
    public string? Language { get; set; }
    public string? Prompt { get; set; }
    public bool EnableTimestamps { get; set; }
    public bool EnableWordLevel { get; set; }
}

/// <summary>
/// 음악 생성 요청
/// </summary>
public class MusicGenerationRequest
{
    public required string Model { get; set; }
    public required string Prompt { get; set; }
    public int DurationSeconds { get; set; } = 30;
    public MusicGenre? Genre { get; set; }
    public MusicMood? Mood { get; set; }
    public int? Tempo { get; set; }
    public AudioFormat Format { get; set; } = AudioFormat.Mp3;
}

/// <summary>
/// 오디오 생성 응답
/// </summary>
public class AudioGenerationResponse
{
    public required GeneratedAudio Audio { get; set; }
    public AudioMetadata? Metadata { get; set; }
}

public class GeneratedAudio
{
    public byte[]? Data { get; set; }
    public string? Url { get; set; }
    public AudioFormat Format { get; set; }
    public int DurationSeconds { get; set; }
    public int SampleRate { get; set; }
}

/// <summary>
/// 음성-텍스트 변환 응답
/// </summary>
public class SpeechToTextResponse
{
    public required string Text { get; set; }
    public string? Language { get; set; }
    public double? Duration { get; set; }
    public IReadOnlyList<TranscriptionSegment>? Segments { get; set; }
}

public class TranscriptionSegment
{
    public required string Text { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
    public double? Confidence { get; set; }
}

public class Voice
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
    public string? Accent { get; set; }
    public string? Language { get; set; }
}

public enum AudioFormat
{
    Mp3,
    Wav,
    Flac,
    Aac,
    Opus,
    Pcm
}

public enum AudioQuality
{
    Low,
    Standard,
    High,
    Lossless
}

public enum MusicGenre
{
    Classical,
    Jazz,
    Rock,
    Pop,
    Electronic,
    Ambient,
    Cinematic
}

public enum MusicMood
{
    Happy,
    Sad,
    Energetic,
    Calm,
    Epic,
    Mysterious
}
```

## HiveServiceBuilder 통합

### IHiveServiceBuilder 확장
```csharp
// IronHive.Abstractions/IHiveServiceBuilder.cs
public interface IHiveServiceBuilder
{
    // ... 기존 메서드들 ...
    
    /// <summary>
    /// 새로운 이미지 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddImageGenerator(string providerName, IImageGenerator generator);
    
    /// <summary>
    /// 새로운 비디오 생성기를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddVideoGenerator(string providerName, IVideoGenerator generator);
    
    /// <summary>
    /// 새로운 오디오 프로세서를 등록합니다.
    /// </summary>
    IHiveServiceBuilder AddAudioProcessor(string providerName, IAudioProcessor processor);
}
```

### HiveServiceBuilder 구현
```csharp
// IronHive.Core/HiveServiceBuilder.cs
public class HiveServiceBuilder : IHiveServiceBuilder
{
    public HiveServiceBuilder(IServiceCollection? services = null)
    {
        // ... 기존 코드 ...
        
        // 새로운 서비스 등록
        Services.TryAddSingleton<IImageService, ImageService>();
        Services.TryAddSingleton<IVideoService, VideoService>();
        Services.TryAddSingleton<IAudioService, AudioService>();
    }
    
    public IHiveServiceBuilder AddImageGenerator(string providerName, IImageGenerator generator)
    {
        _providers.TryAdd<IImageGenerator>(providerName, generator);
        return this;
    }
    
    public IHiveServiceBuilder AddVideoGenerator(string providerName, IVideoGenerator generator)
    {
        _providers.TryAdd<IVideoGenerator>(providerName, generator);
        return this;
    }
    
    public IHiveServiceBuilder AddAudioProcessor(string providerName, IAudioProcessor processor)
    {
        _providers.TryAdd<IAudioProcessor>(providerName, processor);
        return this;
    }
}
```

## Provider 구현 예시 (OpenAI)

### OpenAI Audio Processor
```csharp
// IronHive.Providers.OpenAI/OpenAIAudioProcessor.cs
public class OpenAIAudioProcessor : IAudioProcessor
{
    private readonly OpenAIAudioClient _client;

    public OpenAIAudioProcessor(OpenAIConfig config)
    {
        _client = new OpenAIAudioClient(config);
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    public async Task<AudioGenerationResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = request.ToOpenAIPayload();
        var response = await _client.PostSpeechAsync(payload, cancellationToken);
        return response.ToAudioGenerationResponse();
    }

    public async Task<SpeechToTextResponse> TranscribeAsync(
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default)
    {
        var payload = request.ToOpenAIPayload();
        var response = await _client.PostTranscriptionAsync(payload, cancellationToken);
        return response.ToSpeechToTextResponse();
    }

    public Task<AudioGenerationResponse> GenerateMusicAsync(
        MusicGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("OpenAI does not support music generation.");
    }

    public Task<AudioGenerationResponse> GenerateSoundEffectAsync(
        AudioGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("OpenAI does not support sound effect generation.");
    }
}
```

### Extension Method
```csharp
// IronHive.Providers.OpenAI/Extensions/HiveServiceBuilderExtensions.cs
public static class HiveServiceBuilderExtensions
{
    public static IHiveServiceBuilder AddOpenAIProviders(
        this IHiveServiceBuilder builder,
        string providerName,
        OpenAIConfig config,
        OpenAIServiceType serviceType = OpenAIServiceType.All)
    {
        // ... 기존 코드 ...
        
        if (serviceType.HasFlag(OpenAIServiceType.Images))
            builder.AddImageGenerator(providerName, new OpenAIImageGenerator(config));

        if (serviceType.HasFlag(OpenAIServiceType.Audio))
            builder.AddAudioProcessor(providerName, new OpenAIAudioProcessor(config));

        // if (serviceType.HasFlag(OpenAIServiceType.Videos))
        //     builder.AddVideoGenerator(providerName, new OpenAIVideoGenerator(config));

        return builder;
    }
}
```

### OpenAIServiceType 확장
```csharp
// IronHive.Providers.OpenAI/OpenAIServiceType.cs
[Flags]
public enum OpenAIServiceType
{
    None = 0,
    Models = 1 << 0,
    ChatCompletion = 1 << 1,
    Embeddings = 1 << 2,
    Responses = 1 << 3,
    Images = 1 << 4,
    Audio = 1 << 5,         // 새로 추가
    Videos = 1 << 6,        // 새로 추가
    All = Models | ChatCompletion | Embeddings | Images | Audio
}
```

## 사용 예시

### 비디오 생성 예시
```csharp
var hive = HiveMind.CreateBuilder()
    .AddRunwayProvider("runway", new RunwayConfig { ApiKey = "..." })
    .Build();

var videoService = hive.Services.GetRequiredService<IVideoService>();

// 텍스트로부터 비디오 생성 (비동기 작업)
var response = await videoService.GenerateVideoAsync(
    provider: "runway",
    modelId: "gen-3",
    prompt: "A cat walking on a beach",
    options: new VideoGenerationOptions
    {
        DurationSeconds = 10,
        Resolution = VideoResolution.HD1080p
    });

// 작업 상태 확인
while (response.Status != VideoGenerationStatus.Completed)
{
    await Task.Delay(5000);
    response = await videoService.GetGenerationStatusAsync("runway", response.JobId!);
}

// 비디오 다운로드
var videoData = await new HttpClient().GetByteArrayAsync(response.Video!.Url);
await File.WriteAllBytesAsync("output.mp4", videoData);
```

### 오디오 생성 예시 (TTS)
```csharp
var hive = HiveMind.CreateBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig 
    { 
        ApiKey = "sk-..." 
    }, OpenAIServiceType.Audio)
    .Build();

var audioService = hive.Services.GetRequiredService<IAudioService>();

// 텍스트를 음성으로 변환
var response = await audioService.GenerateSpeechAsync(
    provider: "openai",
    modelId: "tts-1",
    text: "Hello, this is a text to speech example.",
    voice: new Voice 
    { 
        Id = "alloy",
        Name = "Alloy"
    },
    options: new AudioGenerationOptions
    {
        Format = AudioFormat.Mp3,
        Quality = AudioQuality.High,
        Speed = 1.0
    });

await File.WriteAllBytesAsync("speech.mp3", response.Audio.Data!);
```

### 음성 인식 예시 (STT)
```csharp
var audioData = await File.ReadAllBytesAsync("recording.mp3");

var response = await audioService.TranscribeAsync(
    provider: "openai",
    modelId: "whisper-1",
    audio: audioData,
    language: "en",
    options: new TranscriptionOptions
    {
        EnableTimestamps = true,
        EnableWordLevel = true
    });

Console.WriteLine($"Transcription: {response.Text}");

foreach (var segment in response.Segments ?? [])
{
    Console.WriteLine($"[{segment.Start:F2}s - {segment.End:F2}s]: {segment.Text}");
}
```

## 구현 우선순위

### Phase 1: 기본 구조 (최우선)
1. ✅ Abstractions 레이어 인터페이스 정의
2. ✅ Request/Response 모델 정의
3. ✅ Core 서비스 구현
4. ✅ HiveServiceBuilder 확장

### Phase 2: Audio Processor
1. ⬜ IAudioProcessor/IAudioService 구현
2. ⬜ OpenAIAudioProcessor 구현 (TTS, Whisper)
3. ⬜ 테스트 케이스 작성

### Phase 3: Video Generator
1. ⬜ IVideoGenerator/IVideoService 구현
2. ⬜ 비동기 작업 상태 관리
3. ⬜ Provider 구현 (Runway, Pika 등)

### Phase 4: 추가 Provider 지원
1. ⬜ Stability AI (Image)
2. ⬜ Midjourney (Image)
3. ⬜ ElevenLabs (Audio/TTS)
4. ✅ Google Imagen 3 (Image)
5. ⬜ Anthropic (향후 멀티모달 지원 시)

## 고려사항

### 1. 스트리밍 지원
- 오디오/비디오 생성은 일반적으로 비동기 작업
- 상태 폴링 또는 웹훅 기반 알림 고려
- 이미지는 실시간 생성 가능

### 2. 파일 처리
- 대용량 미디어 파일 처리 전략
- 스트림 기반 업로드/다운로드
- 임시 파일 관리

### 3. 비용 관리
- 생성 작업은 비용이 높을 수 있음
- 사용량 추적 및 제한 기능
- 프리뷰 옵션 제공

### 4. 프로바이더 간 차이
- 각 프로바이더의 고유 기능 지원
- 공통 인터페이스로 추상화
- Provider별 옵션은 Dictionary<string, object>로 전달

### 5. 에러 처리
- 타임아웃 처리
- 재시도 로직
- 할당량 초과 처리

## 참고 자료
- OpenAI TTS API: https://platform.openai.com/docs/guides/text-to-speech
- OpenAI Whisper API: https://platform.openai.com/docs/guides/speech-to-text
- Stability AI: https://platform.stability.ai/
- ElevenLabs: https://elevenlabs.io/docs
- Runway: https://runwayml.com/
