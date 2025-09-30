using IronHive.Providers.GoogleAI.Share;
using IronHive.Providers.GoogleAI.Tokens.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI.Tokens;

internal class GoogleAITokensClient : GoogleAIClientBase
{
    public GoogleAITokensClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAITokensClient(string apiKey) : base(apiKey)
    { }

    public async Task<CountTokensResponse> PostCountTokensAsync(
        string modelId,
        CountTokensRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = string.Format(GoogleAIConstants.PostCountTokensPath, modelId).RemovePreffix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<CountTokensResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response content is null or invalid.");
        return res;
    }
}
