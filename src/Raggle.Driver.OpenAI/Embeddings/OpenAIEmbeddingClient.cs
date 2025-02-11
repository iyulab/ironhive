using Raggle.Driver.OpenAI.Base;
using Raggle.Driver.OpenAI.Configurations;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Raggle.Driver.OpenAI.Embeddings;

internal class OpenAIEmbeddingClient : OpenAIClientBase
{
    internal OpenAIEmbeddingClient(string apiKey) : base(apiKey) { }

    internal OpenAIEmbeddingClient(OpenAIConfig config) : base(config) { }

    internal async Task<IEnumerable<EmbeddingResponse>> PostEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OpenAIConstants.PostEmbeddingPath, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        var embeddings = jsonDocument.RootElement.GetProperty("data").EnumerateArray().Select(e =>
        {
            return JsonSerializer.Deserialize<EmbeddingResponse>(e, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize response.");
        });

        return embeddings;
    }
}
