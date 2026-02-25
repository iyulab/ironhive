using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Audio;
using IronHive.Core;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConsoleApp;

/// <summary>
/// 오디오 처리(TTS, STT) 샘플입니다.
/// </summary>
public static class AudioSample
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static async Task Run()
    {
        DotEnv.Load(new DotEnvOptions(
            envFilePaths: [".env"],
            trimValues: true,
            overwriteExistingVars: false
        ));

        var openaiApiKey = Environment.GetEnvironmentVariable("OPENAI")
            ?? throw new InvalidOperationException("OPENAI API 키가 설정되어 있지 않습니다.");
        var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE");

        // HiveServiceBuilder로 오디오 처리 서비스 설정
        var builder = new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig
            {
                ApiKey = openaiApiKey
            }, OpenAIServiceType.Audio)
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = googleApiKey
            }, GoogleAIServiceType.Audio);

        var hive = builder.Build();
        var audioService = hive.Services.GetRequiredService<IAudioService>();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Console.WriteLine("=== 오디오 처리 테스트 ===\n");

        // 영어와 한국어 혼합 텍스트 (약 10초 분량)
        var mixedText = "Welcome to IronHive audio processing. 아이언하이브의 오디오 처리 기능을 테스트합니다. This is a bilingual sample.";

        // === 1. OpenAI Text-to-Speech (TTS) ===
        Console.WriteLine("1. OpenAI TTS - 텍스트를 음성으로 변환...");
        
        var openaiTtsResponse = await audioService.GenerateSpeechAsync(
            provider: "openai",
            request: new TextToSpeechRequest
            {
                Model = "gpt-4o-mini-tts",
                Text = mixedText,
                Voice = "nova",
            });

        var openaiTtsFilePath = Path.Combine(folderPath, $"openai_tts_{DateTime.Now:yyyyMMdd_HHmmss}.mp3");
        File.WriteAllBytes(openaiTtsFilePath, openaiTtsResponse.Audio.Data);
        Console.WriteLine($"   저장: {openaiTtsFilePath}");
        Console.WriteLine($"   크기: {openaiTtsResponse.Audio.Data.Length / 1024.0:F2} KB\n");

        // === 2. OpenAI Speech-to-Text (STT) ===
        Console.WriteLine("2. OpenAI STT - 음성을 텍스트로 변환...");
        
        var openaiAudioData = File.ReadAllBytes(openaiTtsFilePath);
        var openaiSttResponse = await audioService.TranscribeAsync(
            provider: "openai",
            request: new SpeechToTextRequest
            {
                Model = "gpt-4o-transcribe-diarize",
                Audio = new GeneratedAudio
                { 
                    MimeType = "audio/mp3",
                    Data = openaiAudioData,
                }
            });

        Console.WriteLine($"   원본 텍스트: {mixedText}");
        Console.WriteLine($"   변환 결과: \n{JsonSerializer.Serialize(openaiSttResponse, s_jsonOptions)}\n");

        // === 3. Google AI Text-to-Speech (TTS) ===
        Console.WriteLine("3. Google AI TTS - 텍스트를 음성으로 변환...");
                
        var googleTtsResponse = await audioService.GenerateSpeechAsync(
            provider: "google",
            request: new TextToSpeechRequest
            {
                Model = "gemini-2.5-flash-preview-tts",
                Text = mixedText,
                Voice = "Puck",
            });

        var googleTtsFilePath = Path.Combine(folderPath, $"google_tts_{DateTime.Now:yyyyMMdd_HHmmss}.wav");
        File.WriteAllBytes(googleTtsFilePath, googleTtsResponse.Audio.Data);
        Console.WriteLine($"   저장: {googleTtsFilePath}");
        Console.WriteLine($"   크기: {googleTtsResponse.Audio.Data.Length / 1024.0:F2} KB\n");

        // === 4. Google AI Speech-to-Text (STT) ===
        Console.WriteLine("4. Google AI STT - 음성을 텍스트로 변환...");
                
        var googleAudioData = File.ReadAllBytes(googleTtsFilePath);
        var googleSttResponse = await audioService.TranscribeAsync(
            provider: "google",
            request: new SpeechToTextRequest
            {
                Model = "gemini-3-flash-preview",
                Audio = new GeneratedAudio
                { 
                    MimeType = "audio/wav",
                    Data = googleAudioData,
                }
            });

        Console.WriteLine($"   원본 텍스트: {mixedText}");
        Console.WriteLine($"   변환 결과: \n{JsonSerializer.Serialize(googleSttResponse, s_jsonOptions)}\n");

        Console.WriteLine("=== 오디오 처리 테스트 완료 ===");
        Console.WriteLine($"생성된 오디오 파일: {folderPath}");
    }
}
