using Raggle.Engines.OpenAI.Configurations;

namespace Raggle.Engines.OpenAI.Embeddings;

public class OpenAIEmbeddingClient : OpenAIClientBase
{
    public OpenAIEmbeddingClient(string apiKey) : base(apiKey) { }

    public OpenAIEmbeddingClient(OpenAIConfig config) : base(config) { }

    public async Task<IEnumerable<OpenAIModel>> GetEmbeddingModelsAsync() =>
        await GetModelsAsync([OpenAIModelType.Embeddings]);

    public async Task<string> PostEmbeddingAsync(string text, string modelId, CancellationToken cancellationToken = default)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = OpenAIConstants.Host,
            Path = OpenAIConstants.EmbedPath
        }.ToString();

        var request = new OpenAIRequest
        {
            Model = modelId,
            Prompt = text
        };

        var response = await _client.PostAsJsonAsync(requestUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(), cancellationToken: cancellationToken);
        return jsonDocument.RootElement.GetProperty("choices")[0].GetProperty("text").GetString();
    }
}
