using IronHive.Abstractions.Embedding;
using IronHive.Providers.Ollama.Clients;
using IronHive.Providers.Ollama.Payloads.Embedding;

namespace IronHive.Providers.Ollama;

/// <inheritdoc />
public class OllamaEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OllamaEmbeddingClient _client;

    public OllamaEmbeddingGenerator(string baseUrl)
        : this(new OllamaConfig { BaseUrl = baseUrl })
    { }

    public OllamaEmbeddingGenerator(OllamaConfig? config = null)
    {
        _client = new OllamaEmbeddingClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(
        string model, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = model,
            Input = new[] { input }
        }, cancellationToken);

        return res.Embeddings.FirstOrDefault()?.ToArray()
            ?? throw new InvalidOperationException("No embedding found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string model,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new EmbeddingRequest
        {
            Model = model,
            Input = inputs
        }, cancellationToken);

        var result = res.Embeddings.Select((e, i) => new EmbeddingResult
        {
            Index = i,
            Embedding = e
        });
        return result;
    }

    /// <inheritdoc />
    public Task<int> CountTokensAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        // Ollama does not support token counting directly.
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId, 
        IEnumerable<string> inputs, 
        CancellationToken cancellationToken = default)
    {
        // Ollama does not support token counting directly.
        throw new NotImplementedException();
    }
}
