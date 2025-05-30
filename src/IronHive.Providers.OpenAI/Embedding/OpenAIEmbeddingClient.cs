using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Embedding;

internal class OpenAIEmbeddingClient : OpenAIClientBase
{
    internal OpenAIEmbeddingClient(string apiKey) : base(apiKey) { }

    internal OpenAIEmbeddingClient(OpenAIConfig config) : base(config) { }

    internal async Task<IEnumerable<EmbeddingResponse>> PostEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(OpenAIConstants.PostEmbeddingPath.RemovePreffix('/'), content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonDocument = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken) 
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        var embeddings = jsonDocument.RootElement.GetProperty("data").EnumerateArray().Select(el =>
        {
            return el.Deserialize<EmbeddingResponse>(JsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize response.");
        });

        return embeddings;
    }
}
