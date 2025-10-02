using IronHive.Providers.GoogleAI.Payloads.GenerateContent;
using IronHive.Providers.GoogleAI.Payloads.Tokens;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI.Clients;

internal class GoogleAITokensClient : GoogleAIClientBase
{
    public GoogleAITokensClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAITokensClient(string apiKey) : base(apiKey)
    { }

    public async Task<CountTokensResponse> PostCountTokensAsync(
        CountTokensRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Model = request.Model.EnsurePrefix("models/");
        var path = string.Format(GoogleAIConstants.PostCountTokensPath, request.Model).RemovePrefix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<CountTokensResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response content is null or invalid.");
        return res;
    }

    public async Task<CountTokensResponse> PostCountTokensAsync(
        GenerateContentRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Model = request.Model.EnsurePrefix("models/");
        var path = string.Format(GoogleAIConstants.PostCountTokensPath, request.Model).RemovePrefix('/');
        var json = JsonSerializer.Serialize(new { generateContentRequest = request }, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(path, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<CountTokensResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Response content is null or invalid.");
        return res;
    }
}
