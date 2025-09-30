using IronHive.Providers.GoogleAI.EmbedContent.Models;
using IronHive.Providers.GoogleAI.Share;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI.EmbedContent;

internal class GoogleAIEmbedContentClient : GoogleAIClientBase
{
    public GoogleAIEmbedContentClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAIEmbedContentClient(string apiKey) : base(apiKey)
    { }

    public async Task<EmbedContentResponse> PostEmbedContentAsync(
        string modelId, 
        EmbedContentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var path = string.Format(GoogleAIConstants.PostEmbedContentPath, modelId).RemovePreffix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<EmbedContentResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization resulted in null.");
        return res;
    }

    public async Task<BatchEmbedContentResponse> PostBatchEmbedContentAsync(
        string modelId,
        BatchEmbedContentRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = string.Format(GoogleAIConstants.PostBatchEmbedContentPath, modelId).RemovePreffix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();
        
        var res = await response.Content.ReadFromJsonAsync<BatchEmbedContentResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response deserialization resulted in null.");
        return res;
    }
}