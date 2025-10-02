using IronHive.Providers.GoogleAI.Payloads.EmbedContent;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI.Clients;

internal class GoogleAIEmbedContentClient : GoogleAIClientBase
{
    public GoogleAIEmbedContentClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAIEmbedContentClient(string apiKey) : base(apiKey)
    { }

    public async Task<EmbedContentResponse> PostEmbedContentAsync(
        EmbedContentRequest request, 
        CancellationToken cancellationToken = default)
    {
        request.Model = request.Model.EnsurePrefix("models/");
        var path = string.Format(GoogleAIConstants.PostEmbedContentPath, request.Model).RemovePrefix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<EmbedContentResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization resulted in null.");
        return res;
    }

    public async Task<BatchEmbedContentResponse> PostBatchEmbedContentAsync(
        BatchEmbedContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var modelId = string.Empty;
        foreach (var req in request.Requests)
        {
            // "models/" 접두어가 없으면 추가합니다.
            req.Model = req.Model.EnsurePrefix("models/");

            // 첫번째 요청의 모델이름을 기본값으로 사용합니다.
            if (string.IsNullOrWhiteSpace(modelId))
                modelId = req.Model;
        }

        var path = string.Format(GoogleAIConstants.PostBatchEmbedContentPath, modelId).RemovePrefix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<BatchEmbedContentResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization resulted in null.");
        return res;
    }
}