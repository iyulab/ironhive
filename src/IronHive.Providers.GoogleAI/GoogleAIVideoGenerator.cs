using Google.GenAI;
using Google.GenAI.Types;
using IronHive.Abstractions.Videos;
using System.Text.Json;
using GeneratedVideo = IronHive.Abstractions.Videos.GeneratedVideo;

namespace IronHive.Providers.GoogleAI;

/// <summary>
/// Google AI Veo를 사용하여 비디오를 생성하는 클래스입니다.
/// 내부적으로 Job 생성 → 상태 폴링 → 다운로드까지 처리합니다.
/// </summary>
public class GoogleAIVideoGenerator : IVideoGenerator
{
    private readonly Client _client;

    public GoogleAIVideoGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIVideoGenerator(GoogleAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    public GoogleAIVideoGenerator(VertexAIConfig config)
    {
        _client = GoogleAIClientFactory.CreateClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<VideoGenerationResponse> GenerateVideoAsync(
        VideoGenerationRequest request,
        IProgress<VideoGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // 1. 소스 구성
        var source = new GenerateVideosSource
        {
            Prompt = request.Prompt,
            Image = request.Image is { Data.Length: > 0 }
                ? new Image
                {
                    MimeType = request.Image.MimeType,
                    ImageBytes = request.Image.Data,
                }
                : null,
        };

        // 2. 설정 구성
        var preset = GetPresetSize(request.Size);
        var config = new GenerateVideosConfig
        {
            Resolution = preset.Size,
            AspectRatio = preset.Ratio,
            DurationSeconds = request.DurationSeconds,
        };

        // 3. Job 생성
        var operation = await _client.Models.GenerateVideosAsync(
            model: request.Model,
            source: source,
            config: config,
            cancellationToken: cancellationToken);

        if (operation is null)
            throw new InvalidOperationException("Failed to start video generation operation.");

        progress?.Report(new VideoGenerationProgress
        {
            Done = false,
            OperationId = operation.Name,
            Percent = null // not supported by Google AI Veo, so we leave it null
        });

        // 4. 상태 폴링
        while (operation.Done != true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(request.PollInterval, cancellationToken);

            operation = await _client.Operations.GetAsync(operation, null, cancellationToken);
            progress?.Report(new VideoGenerationProgress
            {
                Done = operation.Done == true,
                OperationId = operation.Name,
                Percent = null // not supported by Google AI Veo, so we leave it null
            });
        }

        // 5. 오류 처리
        if (operation.Error != null)
        {
            var errorMessage = JsonSerializer.Serialize(operation.Error);
            errorMessage = string.IsNullOrEmpty(errorMessage) ? "Unknown error" : errorMessage;
            throw new InvalidOperationException($"Video generation failed: {errorMessage}");
        }

        // 6. 비디오 데이터 다운로드
        var video = operation.Response?.GeneratedVideos?.FirstOrDefault()?.Video
            ?? throw new InvalidOperationException("Video generation completed but no video was returned.");
        using var fs = await _client.Files.DownloadAsync(video, null, cancellationToken);
        using var ms = new MemoryStream();
        await fs.CopyToAsync(ms, cancellationToken);
            
        return new VideoGenerationResponse
        {
            Video = new GeneratedVideo
            {
                MimeType = string.IsNullOrEmpty(video.MimeType) ? "video/mp4" : video.MimeType,
                Data = ms.ToArray(),
            }
        };
    }

    private static (string? Size, string? Ratio) GetPresetSize(GeneratedVideoSize? size)
    {
        if (size is GeneratedVideoPresetSize preset)
        {
            return (preset.Resolution, preset.AspectRatio);
        }
        return (null, null);
    }
}
