using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Images;
using IronHive.Core;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

/// <summary>
/// 이미지 생성 및 편집 샘플입니다.
/// </summary>
public static class ImageSample
{
    public static async Task Run()
    {
        var openaiApiKey = Environment.GetEnvironmentVariable("OPENAI")
            ?? throw new InvalidOperationException("OPENAI API KEY is not set in .env file");

        var googleApiKey = Environment.GetEnvironmentVariable("GOOGLE")
            ?? throw new InvalidOperationException("GOOGLE API KEY is not set in .env file");

        // HiveServiceBuilder로 이미지 생성 서비스 설정
        var builder = new HiveServiceBuilder()
            .AddOpenAIProviders("openai", new OpenAIConfig
            {
                ApiKey = openaiApiKey
            }, OpenAIServiceType.Images)
            .AddGoogleAIProviders("google", new GoogleAIConfig
            {
                ApiKey = googleApiKey
            }, GoogleAIServiceType.Images);

        var hive = builder.Build();

        var imageService = hive.Services.GetRequiredService<IImageService>();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Console.WriteLine("=== 이미지 생성 테스트 ===\n");

        // === 1. OpenAI GPT-Image-2 모델로 이미지 생성 ===
        Console.WriteLine("\n1. GPT-Image-2로 고품질 이미지 생성...");
        var gptImageRes = await imageService.GenerateImageAsync(
            provider: "openai",
            request: new ImageGenerationRequest
            {
                Model = "gpt-image-2",
                Prompt = "A serene Korean palace garden with cherry blossoms and a traditional pavilion, photorealistic style",
                Size = new GeneratedImagePixelSize
                {
                    Width = 1024,
                    Height = 1024
                },
            });

        foreach (var img in gptImageRes.Images)
        {
            var fileName = $"{folderPath}\\gpt_image_output_{Guid.NewGuid()}.png";
            File.WriteAllBytes(fileName, img.Data);
            Console.WriteLine($"   저장: {fileName}");
        }

        // === 2. Google Imagen-4.0로 이미지 생성 ===
        Console.WriteLine("\n2. Imagen-4.0로 이미지 생성...");
        var imageneRes = await imageService.GenerateImageAsync(
            provider: "google",
            request: new ImageGenerationRequest
            {
                Model = "imagen-4.0-generate-001",
                Prompt = "A vibrant Korean street market at night with neon signs and bustling crowds, cinematic style",
                N = 2,
                Size = new GeneratedImageScaleSize
                {
                    Resolution = "2k",
                    AspectRatio = "1:1"
                },
            });

        foreach (var img in imageneRes.Images)
        {
            var fileName = $"{folderPath}\\imagene_output_{Guid.NewGuid()}.png";
            File.WriteAllBytes(fileName, img.Data);
            Console.WriteLine($"   저장: {fileName}");
        }

        // === 3. Google Gemini-2.5-Flash-Image로 이미지 생성 ===
        Console.WriteLine("\n3. Gemini-2.5-Flash-Image로 이미지 생성...");
        var geminiRes = await imageService.GenerateImageAsync(
            provider: "google",
            request: new ImageGenerationRequest
            {
                Model = "gemini-2.5-flash-image",
                Prompt = "A vibrant Korean street market at night with neon signs and bustling crowds, cinematic style",
                N = 2,
                Size = new GeneratedImageScaleSize
                {
                    Resolution = "4k",
                    AspectRatio = "1:1"
                },
            });

        foreach (var img in geminiRes.Images)
        {
            var fileName = $"{folderPath}\\gemini_output_{Guid.NewGuid()}.png";
            File.WriteAllBytes(fileName, img.Data);
            Console.WriteLine($"   저장: {fileName}");
        }

        Console.WriteLine("\n=== 이미지 편집 테스트 ===\n");

        // === 4. OpenAI gpt-image-2로 이미지 편집 (인페인팅) ===
        Console.WriteLine("4. gpt-image-2로 이미지 편집 (인페인팅)...");
        if (gptImageRes.Images.Count > 0)
        {
            var originalImage = gptImageRes.Images.First();

            var editRes = await imageService.EditImageAsync(
                provider: "openai",
                request: new ImageEditRequest
                {
                    Model = "gpt-image-2",
                    Prompt = "Add a traditional Korean hanbok-clad figure standing in the garden, facing away from the viewer",
                    Images = [originalImage],
                    N = 1,
                    Size = new GeneratedImagePixelSize
                    {
                        Width = 1024,
                        Height = 1024
                    },
                });

            foreach (var img in editRes.Images)
            {
                var fileName = $"{folderPath}\\gpt_image_edit_{Guid.NewGuid()}.png";
                File.WriteAllBytes(fileName, img.Data);
                Console.WriteLine($"   저장: {fileName}");
            }
        }

        // === 5. Google Gemini-2.5-Flash-Image로 이미지 편집 ===
        Console.WriteLine("\n5. Gemini-2.5-Flash-Image로 이미지 편집...");
        if (geminiRes.Images.Count > 0)
        {
            var originalImage = geminiRes.Images.First();

            var geminiEditRes = await imageService.EditImageAsync(
                provider: "google",
                request: new ImageEditRequest
                {
                    Model = "gemini-2.5-flash-image",
                    Prompt = "Make it snow in the street market scene",
                    Images = [originalImage],
                    N = 1,
                    Size = new GeneratedImageScaleSize
                    {
                        Resolution = "4k",
                        AspectRatio = "1:1"
                    },
                });

            foreach (var img in geminiEditRes.Images)
            {
                var fileName = $"{folderPath}\\gemini_edit_{Guid.NewGuid()}.png";
                File.WriteAllBytes(fileName, img.Data);
                Console.WriteLine($"   저장: {fileName}");
            }
        }

        Console.WriteLine("\n샘플 실행 완료!");
        Console.WriteLine($"모든 이미지는 다음 폴더에 저장되었습니다: {folderPath}");
    }
}
