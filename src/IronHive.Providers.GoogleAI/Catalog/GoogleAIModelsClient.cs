using IronHive.Providers.GoogleAI.Catalog.Models;
using IronHive.Providers.GoogleAI.Share;
using System.Net.Http.Json;

namespace IronHive.Providers.GoogleAI.Catalog;

public class GoogleAIModelsClient : GoogleAIClientBase
{
    public GoogleAIModelsClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAIModelsClient(string apiKey) : base(apiKey)
    { }

    /// <summary>
    /// 지정한 모델 ID에 대한 Google AI 모델을 가져옵니다.
    /// </summary>
    public async Task<GoogleAIModel?> GetModelAsync(
        string modelId,
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(GoogleAIConstants.GetModelsListPath, modelId).RemovePreffix('/');
        var model = await _client.GetFromJsonAsync<GoogleAIModel>(path, _jsonOptions, cancellationToken);
        return model;
    }

    /// <summary>
    /// 요청에 지정된 매개변수에 따라 사용 가능한 Google AI 모델 목록을 가져옵니다.
    /// </summary>
    public async Task<GoogleAIModelsResponse> GetModelsAsync(
        GoogleAIModelsRequest request, 
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();
        if (request.PageSize > 0)
            query["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrWhiteSpace(request.PageToken))
            query["pageToken"] = request.PageToken;

        var path = GoogleAIConstants.GetModelsListPath.RemovePreffix('/');
        if (query.Count != 0)
        {
            path += "?"; 
            path += string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));
        }

        var response = await _client.GetFromJsonAsync<GoogleAIModelsResponse>(path, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve models from Anthropic API.");
        return response;
    }
}