using dotenv.net;
using IronHive.Abstractions;
using IronHive.Abstractions.Images;
using IronHive.Core;
using IronHive.Providers.GoogleAI;
using IronHive.Providers.OpenAI;
using IronHive.Providers.OpenAI.Compatible.XAI;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

/// <summary>
/// 이미지 생성 및 편집 샘플입니다.
/// </summary>
public static class ImageSample
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

        var xaiApiKey = Environment.GetEnvironmentVariable("XAI");

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

        // XAI API 키가 있으면 XAI 추가
        if (!string.IsNullOrEmpty(xaiApiKey))
        {
            builder.AddXAIProviders("xai", new XAIConfig
            {
                ApiKey = xaiApiKey
            }, XAIServiceType.Images);
        }

        var hive = builder.Build();

        var imageService = hive.Services.GetRequiredService<IImageService>();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Console.WriteLine("=== 이미지 생성 테스트 ===\n");

        // === 1. OpenAI DALL-E 3로 이미지 생성 ===
        Console.WriteLine("1. DALL-E 3로 이미지 생성...");
        var dalleRes = await imageService.GenerateImageAsync(
            provider: "openai",
            request: new ImageGenerationRequest
            {
                Model = "dall-e-3",
                Prompt = "A majestic dragon flying over a futuristic Korean city at sunset, digital art style",
                N = 1,
                Size = new GeneratedImageCustomSize
                {
                    Value = "1024x1024"
                },
            });

        foreach (var img in dalleRes.Images)
        {
            var fileName = $"{folderPath}\\dalle_output_{Guid.NewGuid()}.png";
            File.WriteAllBytes(fileName, img.Data);
            Console.WriteLine($"   저장: {fileName}");
        }

        // === 2. OpenAI GPT-Image-1.5 모델로 이미지 생성 ===
        Console.WriteLine("\n2. GPT-Image-1.5로 고품질 이미지 생성...");
        var gptImageRes = await imageService.GenerateImageAsync(
            provider: "openai",
            request: new ImageGenerationRequest
            {
                Model = "gpt-image-1.5",
                Prompt = "A serene Korean palace garden with cherry blossoms and a traditional pavilion, photorealistic style",
                Size = new GeneratedImageCustomSize
                {
                    Value = "1024x1024"
                },
            });

        foreach (var img in gptImageRes.Images)
        {
            var fileName = $"{folderPath}\\gpt_image_output_{Guid.NewGuid()}.png";
            File.WriteAllBytes(fileName, img.Data);
            Console.WriteLine($"   저장: {fileName}");
        }

        // === 3. Google Imagen-4.0로 이미지 생성 ===
        Console.WriteLine("\n3. Imagen-4.0로 이미지 생성...");
        var imageneRes = await imageService.GenerateImageAsync(
            provider: "google",
            request: new ImageGenerationRequest
            {
                Model = "imagen-4.0-generate-001",
                Prompt = "A vibrant Korean street market at night with neon signs and bustling crowds, cinematic style",
                N = 2,
                Size = new GeneratedImagePresetSize
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

        // === 4. Google Gemini-2.5-Flash-Image로 이미지 생성 ===
        Console.WriteLine("\n4. Gemini-2.5-Flash-Image로 이미지 생성...");
        var geminiRes = await imageService.GenerateImageAsync(
            provider: "google",
            request: new ImageGenerationRequest
            {
                Model = "gemini-2.5-flash-image",
                Prompt = "A vibrant Korean street market at night with neon signs and bustling crowds, cinematic style",
                N = 2,
                Size = new GeneratedImagePresetSize
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

        // === 5. OpenAI gpt-image-1-mini로 이미지 편집 (인페인팅) ===
        Console.WriteLine("5. gpt-image-1-mini로 이미지 편집 (인페인팅)...");
        
        // 원본 이미지 로드 (첫 번째 생성된 이미지 사용)
        if (gptImageRes.Images.Count > 0)
        {
            var originalImage = gptImageRes.Images.First();
            
            // 마스크 생성 (실제로는 사용자가 제공하거나 별도 생성 필요)
            // 여기서는 간단히 원본의 중앙 영역을 편집하는 예시
            var editRes = await imageService.EditImageAsync(
                provider: "openai",
                request: new ImageEditRequest
                {
                    Model = "gpt-image-1-mini",
                    Prompt = "Add a traditional Korean hanbok-clad figure standing in the garden, facing away from the viewer",
                    Images = [originalImage],
                    // Mask = null, // 마스크 없이 전체 편집
                    N = 1,
                    Size = new GeneratedImageCustomSize
                    {
                        Value = "1024x1024"
                    },
                });

            foreach (var img in editRes.Images)
            {
                var fileName = $"{folderPath}\\gpt_image_edit_{Guid.NewGuid()}.png";
                File.WriteAllBytes(fileName, img.Data);
                Console.WriteLine($"   저장: {fileName}");
            }
        }

        // === 6. Google Gemini-2.5-Flash-Image로 이미지 편집 ===
        Console.WriteLine("\n6. Gemini-2.5-Flash-Image로 이미지 편집...");
        
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
                    Size = new GeneratedImagePresetSize
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

        // === 8. XAI grok-imagine-image으로 이미지 생성 (API 키가 있는 경우) ===
        if (!string.IsNullOrEmpty(xaiApiKey))
        {
            Console.WriteLine("\n8. XAI grok-imagine-image으로 이미지 생성...");
            var xaiRes = await imageService.GenerateImageAsync(
                provider: "xai",
                request: new ImageGenerationRequest
                {
                    Model = "grok-imagine-image",
                    Prompt = "A futuristic AI robot exploring a Korean traditional hanok village at sunset",
                    N = 1,
                    Size = new GeneratedImageCustomSize
                    {
                        Value = "1024x1024"
                    },
                });

            foreach (var img in xaiRes.Images)
            {
                var fileName = $"{folderPath}\\xai_output_{Guid.NewGuid()}.png";
                File.WriteAllBytes(fileName, img.Data);
                Console.WriteLine($"   저장: {fileName}");
            }
        }

        Console.WriteLine("\n샘플 실행 완료!");
        Console.WriteLine($"모든 이미지는 다음 폴더에 저장되었습니다: {folderPath}");
    }
}
