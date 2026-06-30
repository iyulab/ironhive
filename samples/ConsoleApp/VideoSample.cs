using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Videos;
using IronHive.Core;
using IronHive.Providers.GoogleAI;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

/// <summary>
/// 비디오 생성 샘플입니다.
/// Google Veo를 사용하여 Text-to-Video를 시연합니다.
/// </summary>
public static class VideoSample
{
    public static async Task Run()
    {
        var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE")
            ?? throw new InvalidOperationException("GOOGLE API KEY is not set in .env file");

        // HiveServiceBuilder로 비디오 생성 서비스 설정
        var hive = new HiveServiceBuilder()
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = googleApiKey
            }, GoogleAIServiceType.Videos)
            .Build();

        var videoService = hive.Videos;
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Console.WriteLine("=== 비디오 생성 테스트 ===\n");

        // 진행 상태 콜백
        var progress = new Progress<VideoGenerationProgress>(p =>
            Console.WriteLine($"  Progress: {(p.Percent.HasValue ? $"{p.Percent}%" : "...")} (ID: {p.OperationId})"));

        // === Google Veo로 Text-to-Video ===
        Console.WriteLine("1. Google Veo (veo-3.1-generate-preview)로 Text-to-Video 생성...");
        //var veoRes = await videoService.GenerateVideoAsync(
        //    provider: "google",
        //    request: new VideoGenerationRequest
        //    {
        //        Model = "veo-3.1-generate-preview",
        //        Prompt = "A snowboarder jumping off a snowy mountain cliff, ultra slow motion, snow particles flying, action camera perspective",
        //        Size = new GeneratedVideoPresetSize
        //        {
        //            Resolution = "720p",
        //            AspectRatio = "16:9"
        //        },
        //        DurationSeconds = 4,
        //    },
        //    progress: progress);

        var fileName = $"{folderPath}\\veo_output_{Guid.NewGuid()}.mp4";
        //File.WriteAllBytes(fileName, veoRes.Video.Data);
        Console.WriteLine($"   저장: {fileName}");

        Console.WriteLine("\n샘플 실행 완료!");
        Console.WriteLine($"모든 비디오는 다음 폴더에 저장되었습니다: {folderPath}");
    }
}
