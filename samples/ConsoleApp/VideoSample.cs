using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Videos;
using IronHive.Core;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

/// <summary>
/// 비디오 생성 샘플입니다.
/// OpenAI Sora와 Google Veo를 사용하여 Text-to-Video, Image-to-Video를 시연합니다.
/// </summary>
public static class VideoSample
{
    public static async Task Run()
    {
        DotEnv.Load(new DotEnvOptions(
            envFilePaths: [".env"],
            trimValues: true,
            overwriteExistingVars: false
        ));

        var openaiApiKey = Environment.GetEnvironmentVariable("OPENAI")
            ?? throw new InvalidOperationException("OPENAI API KEY is not set in .env file");

        var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE")
            ?? throw new InvalidOperationException("GOOGLE API KEY is not set in .env file");

        // HiveServiceBuilder로 비디오 생성 서비스 설정
        var hive = new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig
            {
                ApiKey = openaiApiKey
            }, OpenAIServiceType.Videos)
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = googleApiKey
            }, GoogleAIServiceType.Videos)
            .Build();

        var videoService = hive.Services.GetRequiredService<IVideoService>();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Console.WriteLine("=== 비디오 생성 테스트 ===\n");

        // 진행 상태 콜백
        var progress = new Progress<VideoGenerationProgress>(p =>
            Console.WriteLine($"  Progress: {(p.Percent.HasValue ? $"{p.Percent}%" : "...")} (ID: {p.OperationId})"));

        // === 1. OpenAI Sora로 Text-to-Video ===
        Console.WriteLine("1. OpenAI Sora (sora-2)로 Text-to-Video 생성...");
        //var soraRes = await videoService.GenerateVideoAsync(
        //    provider: "openai",
        //    request: new VideoGenerationRequest
        //    {
        //        Model = "sora-2",
        //        Prompt = "A vast desert with a futuristic city floating above the horizon, heat distortion in the air, camera slowly tilts upward, epic cinematic style",
        //        Size = new GeneratedVideoCustomSize
        //        {
        //            Value = "1280x720"
        //        },
        //        DurationSeconds = 4,
        //    },
        //    progress: progress);

        var fileName = $"{folderPath}\\sora_output_{Guid.NewGuid()}.mp4";
        //File.WriteAllBytes(fileName, soraRes.Video.Data);
        Console.WriteLine($"   저장: {fileName}");

        // === 2. Google Veo로 Text-to-Video ===
        Console.WriteLine("\n2. Google Veo (veo-3.1-generate-preview)로 Text-to-Video 생성...");
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

        fileName = $"{folderPath}\\veo_output_{Guid.NewGuid()}.mp4";
        //File.WriteAllBytes(fileName, veoRes.Video.Data);
        Console.WriteLine($"   저장: {fileName}");

        // === 3. OpenAI Sora로 Image-to-Video ===
        Console.WriteLine("\n3. OpenAI Sora (sora-2)로 Image-to-Video 생성...");
        if (File.Exists("dragon.jpg"))
        {
            var imageData = File.ReadAllBytes("dragon.jpg");
            var imageToVideoRes = await videoService.GenerateVideoAsync(
                provider: "openai",
                request: new VideoGenerationRequest
                {
                    Model = "sora-2",
                    Prompt = "A powerful ice dragon standing calmly on a glacier. It slowly raises its head as snow swirls around it. The camera gently moves from a close-up to a wide shot. Soft blue cinematic lighting, high detail, fantasy style.",
                    Image = new IronHive.Abstractions.Images.GeneratedImage
                    {
                        MimeType = "image/jpeg",
                        Data = imageData,
                    },
                    Size = new GeneratedVideoCustomSize
                    {
                        Value = "1280x720"
                    },
                    DurationSeconds = 4,
                },
                progress: progress);

            fileName = $"{folderPath}\\sora_image_to_video_{Guid.NewGuid()}.mp4";
            File.WriteAllBytes(fileName, imageToVideoRes.Video.Data);
            Console.WriteLine($"   저장: {fileName}");
        }
        else
        {
            Console.WriteLine("   dragon.jpg 파일이 없어 Image-to-Video를 건너뜁니다.");
        }

        Console.WriteLine("\n샘플 실행 완료!");
        Console.WriteLine($"모든 비디오는 다음 폴더에 저장되었습니다: {folderPath}");
    }
}
