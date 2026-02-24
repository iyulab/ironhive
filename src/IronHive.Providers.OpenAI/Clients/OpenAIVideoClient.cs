using IronHive.Providers.OpenAI.Payloads.Videos;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace IronHive.Providers.OpenAI.Clients;

public class OpenAIVideoClient : OpenAIClientBase
{
    public OpenAIVideoClient(OpenAIConfig config) : base(config) { }

    /// <summary>
    /// 비디오 생성 작업을 제출합니다. (POST /v1/videos)
    /// multipart/form-data 형식으로 전송합니다.
    /// </summary>
    public async Task<CreateVideoResponse> CreateVideoAsync(
        CreateVideoRequest request,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        if (!string.IsNullOrEmpty(request.Prompt))
            content.Add(new StringContent(request.Prompt), "prompt");

        if (request.Image?.Data != null && !string.IsNullOrEmpty(request.Image.MimeType))
        {
            var imgContent = new ByteArrayContent(request.Image.Data);
            imgContent.Headers.ContentType = new MediaTypeHeaderValue(request.Image.MimeType);
            var extension = request.Image.MimeType switch
            {
                "image/jpeg" => "jpg",
                "image/png" => "png",
                "image/webp" => "webp",
                _ => "bin"
            };
            content.Add(imgContent, "input_reference", $"input_reference.{extension}");
        }

        content.Add(new StringContent(request.Model), "model");

        if (request.Seconds.HasValue)
            content.Add(new StringContent(request.Seconds.Value.ToString(CultureInfo.InvariantCulture)), "seconds");

        if (!string.IsNullOrEmpty(request.Size))
            content.Add(new StringContent(request.Size), "size");
        
        using var response = await Client.PostAsync(
            OpenAIConstants.PostVideosPath.RemovePrefix('/'),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateVideoResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize video creation response.");
    }

    /// <summary>
    /// 비디오 상태를 조회합니다. (GET /v1/videos/{videoId})
    /// </summary>
    public async Task<CreateVideoResponse> RetrieveVideoAsync(
        string videoId,
        CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync(
            $"{OpenAIConstants.PostVideosPath.RemovePrefix('/')}/{videoId}",
            cancellationToken);

        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<CreateVideoResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize video status response.");
    }

    /// <summary>
    /// 완성된 비디오를 다운로드합니다. (GET /v1/videos/{videoId}/content?variant=video)
    /// </summary>
    public async Task<byte[]> DownloadVideoContentAsync(
        string videoId,
        CancellationToken cancellationToken = default)
    {
        using var response = await Client.GetAsync(
            $"{OpenAIConstants.PostVideosPath.RemovePrefix('/')}/{videoId}/content?variant=video",
            cancellationToken);

        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
