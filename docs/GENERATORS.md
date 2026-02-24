# Generator Architecture Design

## 개요
IronHive의 Generator 아키텍처는 일관된 패턴으로 다양한 AI 생성 기능을 제공합니다. 
기존의 MessageGenerator와 EmbeddingGenerator 패턴을 확장하여 Image, Video 생성 및 Audio 처리(TTS/STT) 기능을 추가합니다.

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
│   - XXXAudioProcessor                    │
└─────────────────────────────────────────┘
```

### 핵심 원칙
1. **Provider Interface (IXxxGenerator)**: 프로바이더 구현자를 위한 저수준 인터페이스
2. **Application Interface (IXxxService)**: 애플리케이션 개발자를 위한 고수준 인터페이스
3. **Request/Response Pattern**: 명확한 입력/출력 계약
4. **Provider Registry**: 다중 프로바이더를 통합 관리
5. **DI Integration**: Microsoft.Extensions.DependencyInjection 기반

## AudioProcessor 설계

#### 파일 구조
```
src/IronHive.Abstractions/Audio/
├── IAudioProcessor.cs
├── IAudioService.cs
├── TextToSpeechRequest.cs
├── SpeechToTextRequest.cs
├── AudioGenerationResponse.cs
├── GeneratedAudio.cs
├── SpeechToTextResponse.cs
└── TranscriptionSegment.cs

src/IronHive.Core/Services/
├── AudioService.cs

src/IronHive.Providers.OpenAI/
├── OpenAIAudioProcessor.cs (TTS: gpt-4o-mini-tts/tts-1, STT: whisper-1/gpt-4o-transcribe)
```

#### 인터페이스 정의
```csharp
namespace IronHive.Abstractions.Audio;

/// <summary>
/// 오디오 처리 기능(TTS, STT)을 제공하는 프로바이더 인터페이스입니다.
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
}

/// <summary>
/// 오디오 처리 서비스를 제공하는 애플리케이션 인터페이스입니다.
/// provider만 추가하고 request를 그대로 전달합니다.
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// 텍스트를 음성으로 변환합니다. (TTS)
    /// </summary>
    Task<AudioGenerationResponse> GenerateSpeechAsync(
        string provider,
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 음성을 텍스트로 변환합니다. (STT)
    /// </summary>
    Task<SpeechToTextResponse> TranscribeAsync(
        string provider,
        SpeechToTextRequest request,
        CancellationToken cancellationToken = default);
}
```

#### Request/Response 모델
```csharp
/// <summary>
/// 텍스트를 음성으로 변환 요청 (TTS).
///
/// API 매핑:
///   OpenAI → POST /v1/audio/speech (model, input, voice, response_format, speed)
///   Google → TTS API (model, text, voice, audioConfig)
/// </summary>
public class TextToSpeechRequest
{
    /// <summary>모델 ID (e.g. "tts-1", "tts-1-hd", "gpt-4o-mini-tts")</summary>
    public required string Model { get; set; }

    /// <summary>변환할 텍스트</summary>
    public required string Text { get; set; }

    /// <summary>음성 ID (e.g. "alloy", "echo", "nova", "shimmer")</summary>
    public required string Voice { get; set; }

    /// <summary>출력 포맷 (e.g. "mp3", "wav", "opus", "aac", "flac", "pcm")</summary>
    public string? ResponseFormat { get; set; }

    /// <summary>재생 속도. OpenAI: 0.25~4.0, 기본값 1.0</summary>
    public double? Speed { get; set; }
}

/// <summary>
/// 음성을 텍스트로 변환 요청 (STT).
///
/// API 매핑:
///   OpenAI → POST /v1/audio/transcriptions (model, file, language, prompt, temperature, timestamp_granularities[])
/// </summary>
public class SpeechToTextRequest
{
    /// <summary>모델 ID (e.g. "whisper-1", "gpt-4o-transcribe", "gpt-4o-mini-transcribe")</summary>
    public required string Model { get; set; }

    /// <summary>오디오 데이터 (mp3, wav, flac, ogg, webm 등)</summary>
    public required byte[] Audio { get; set; }

    /// <summary>오디오 MIME 타입 (e.g. "audio/mp3", "audio/wav")</summary>
    public string? MimeType { get; set; }

    /// <summary>언어 힌트 (ISO-639-1, e.g. "en", "ko")</summary>
    public string? Language { get; set; }

    /// <summary>문맥 힌트 프롬프트 (용어, 스타일 가이드)</summary>
    public string? Prompt { get; set; }

    /// <summary>샘플링 온도 (0~1). 0이면 결정적 출력.</summary>
    public double? Temperature { get; set; }
}

/// <summary>
/// TTS 응답. 바이너리 오디오 데이터를 포함합니다.
/// </summary>
public class AudioGenerationResponse
{
    public required GeneratedAudio Audio { get; set; }
}

public class GeneratedAudio
{
    public byte[] Data { get; set; } = [];
    public string? MimeType { get; set; }
}

/// <summary>
/// STT 응답. 변환된 텍스트와 선택적 세그먼트를 포함합니다.
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

### 오디오 생성 예시 (TTS)
```csharp
var hive = HiveMind.CreateBuilder()
    .AddOpenAIProviders("openai", new OpenAIConfig { ApiKey = "sk-..." },
        OpenAIServiceType.Audio)
    .Build();

var audioService = hive.Services.GetRequiredService<IAudioService>();

// 텍스트를 음성으로 변환
var response = await audioService.GenerateSpeechAsync(
    provider: "openai",
    request: new TextToSpeechRequest
    {
        Model = "tts-1",
        Text = "Hello, this is a text to speech example.",
        Voice = "alloy",
        ResponseFormat = "mp3",
        Speed = 1.0
    });

await File.WriteAllBytesAsync("speech.mp3", response.Audio.Data);
```

### 음성 인식 예시 (STT)
```csharp
var audioData = await File.ReadAllBytesAsync("recording.mp3");

var response = await audioService.TranscribeAsync(
    provider: "openai",
    request: new SpeechToTextRequest
    {
        Model = "whisper-1",
        Audio = audioData,
        MimeType = "audio/mp3",
        Language = "en"
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
- OpenAI Sora (Video): https://developers.openai.com/api/docs/guides/video-generation/
- OpenAI Audio (TTS/STT): https://platform.openai.com/docs/guides/audio
- Google Veo (Video): https://ai.google.dev/gemini-api/docs/video
- Stability AI: https://platform.stability.ai/
- ElevenLabs: https://elevenlabs.io/docs
