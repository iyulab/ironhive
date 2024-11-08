using Raggle.Connector.Ollama.Base;
using Raggle.Connector.Ollama.Configurations;
using Raggle.Connector.Ollama.Embeddings.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Raggle.Connector.Ollama.Embeddings;

internal class OllamaEmbeddingClient : OllamaClientBase
{
    internal OllamaEmbeddingClient(OllamaConfig? config = null) : base(config) { }

    internal OllamaEmbeddingClient(string endPoint) : base(endPoint) { }

    internal async Task<IEnumerable<OllamaModel>> GetEmbeddingModelsAsync(
        CancellationToken cancellationToken)
    {
        // Ollama does not have a information about model categories
        return await GetModelsAsync(cancellationToken);
    }

    internal async Task<EmbeddingResponse> PostEmbeddingAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(OllamaConstants.PostEmbeddingPath, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var embeddings = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken)
            ?? throw new HttpRequestException("Failed to deserialize response");
        return embeddings;
    }
}
