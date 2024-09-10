using Raggle.Engines.OpenAI.Abstractions;
using Raggle.Engines.OpenAI.Configurations;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Raggle.Engines.OpenAI.Embeddings;

public class OpenAIEmbeddingClient : OpenAIClientBase
{
    public OpenAIEmbeddingClient(string apiKey) : base(apiKey) { }

    public OpenAIEmbeddingClient(OpenAIConfig config) : base(config) { }

    public async Task<IEnumerable<OpenAIEmbeddingModel>> GetEmbeddingModelsAsync()
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

    public async Task<IEnumerable<EmbeddingResponse>> PostEmbeddingAsync(EmbeddingRequest request)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = OpenAIConstants.Host,
            Path = OpenAIConstants.PostEmbeddingPath
        }.ToString();

        var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(requestUri, content);
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
