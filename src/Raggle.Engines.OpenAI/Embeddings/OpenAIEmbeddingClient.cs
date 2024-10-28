using Raggle.Engines.OpenAI.Abstractions;
using Raggle.Engines.OpenAI.Configurations;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Raggle.Engines.OpenAI.Embeddings;

internal class OpenAIEmbeddingClient : OpenAIClientBase
{
    internal OpenAIEmbeddingClient(string apiKey) : base(apiKey) { }

    internal OpenAIEmbeddingClient(OpenAIConfig config) : base(config) { }

    internal async Task<IEnumerable<OpenAIEmbeddingModel>> GetEmbeddingModelsAsync()
    {
        var models = await GetModelsAsync();
        return models.Where(OpenAIModel.IsEmbeddingModel)
                     .Select(m => new OpenAIEmbeddingModel
                     {
                         ID = m.ID,
                         Created = m.Created,
                         OwnedBy = m.OwnedBy
                     });
    }

    internal async Task<IEnumerable<EmbeddingResponse>> PostEmbeddingAsync(EmbeddingRequest request)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(OpenAIConstants.PostEmbeddingPath, content);
        response.EnsureSuccessStatusCode();

        var jsonDocument = await response.Content.ReadFromJsonAsync<JsonDocument>() 
            ?? throw new InvalidOperationException("Failed to deserialize response.");

        var embeddings = jsonDocument.RootElement.GetProperty("data").EnumerateArray().Select(e =>
        {
            return JsonSerializer.Deserialize<EmbeddingResponse>(e, _jsonOptions)!;
        });

        return embeddings;
    }
}
