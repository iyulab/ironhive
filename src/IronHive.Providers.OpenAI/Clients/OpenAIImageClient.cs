using IronHive.Providers.OpenAI.Payloads.Images;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Clients;

public class OpenAIImageClient : OpenAIClientBase
{
    public OpenAIImageClient(string apiKey) : base(apiKey) { }

    public OpenAIImageClient(OpenAIConfig config) : base(config) { }

    /// <summary>
    /// 텍스트 프롬프트로부터 이미지를 생성합니다.
    /// </summary>
    public async Task<CreateImagesResponse> PostCreateImagesAsync(
        CreateImagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(
            OpenAIConstants.PostImagesGenerationsPath.RemovePrefix('/'), 
            content, 
            cancellationToken);
        
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<CreateImagesResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        return res;
    }

    /// <summary>
    /// 이미지를 편집합니다.
    /// </summary>
    public async Task<EditImagesResponse> PostEditImagesAsync(
        EditImagesRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(
            OpenAIConstants.PostImagesEditsPath.RemovePrefix('/'),
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<EditImagesResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        return res;
    }
}
