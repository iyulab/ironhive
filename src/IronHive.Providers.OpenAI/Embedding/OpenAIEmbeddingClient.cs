using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Embedding;

public class OpenAIEmbeddingClient : OpenAIClientBase
{
    public OpenAIEmbeddingClient(string apiKey) : base(apiKey) { }

    public OpenAIEmbeddingClient(OpenAIConfig config) : base(config) { }

    public async Task<OpenAIEmbeddingResponse> PostEmbeddingAsync(
        OpenAIEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OpenAIConstants.PostEmbeddingPath.RemovePreffix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var res = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        return res;
    }
}
