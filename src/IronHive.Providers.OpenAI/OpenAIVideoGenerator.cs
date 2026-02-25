using IronHive.Abstractions.Videos;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.Videos;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// OpenAI Sora를 사용하여 비디오를 생성하는 클래스입니다.
/// 내부적으로 Job 생성 → 상태 폴링 → 다운로드까지 처리합니다.
/// </summary>
public class OpenAIVideoGenerator : IVideoGenerator
{
    private readonly OpenAIVideoClient _client;

    public OpenAIVideoGenerator(OpenAIConfig config)
    {
        _client = new OpenAIVideoClient(config);
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
        var payload = new CreateVideoRequest
        {
            Model = request.Model,
            Prompt = request.Prompt,
            Image = request.Image,
            Seconds = request.DurationSeconds,
            Size = request.Size is GeneratedVideoCustomSize customSize
                ? customSize.Value
                : null
        };

        // 1. Job 생성
        var response = await _client.CreateVideoAsync(
            payload,
            cancellationToken);

        var videoId = response.Id
            ?? throw new InvalidOperationException("Video creation did not return an ID.");

        progress?.Report(new VideoGenerationProgress
        {
            Done = false,
            OperationId = videoId,
            Percent = response.Progress,
        });

        // 2. 상태 폴링
        while (response.Status is not (CreateVideoStatus.Completed or CreateVideoStatus.Failed))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(request.PollInterval, cancellationToken);

            response = await _client.RetrieveVideoAsync(videoId, cancellationToken);

            progress?.Report(new VideoGenerationProgress
            {
                Done = response.Status == CreateVideoStatus.Completed,
                OperationId = videoId,
                Percent = response.Progress,
            });
        }

        // 3. 실패 처리
        if (response.Status == CreateVideoStatus.Failed)
        {
            var errorMessage = response.Error?.Message ?? "Unknown error occurred";
            throw new InvalidOperationException($"Video generation failed: {errorMessage}");
        }

        // 4. 비디오 다운로드
        var content = await _client.DownloadVideoContentAsync(videoId, cancellationToken);

        return new VideoGenerationResponse
        {
            Video = new GeneratedVideo
            {
                MimeType = "video/mp4",
                Data = content,
            }
        };
    }
}
