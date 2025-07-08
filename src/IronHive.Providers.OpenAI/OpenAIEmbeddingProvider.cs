using IronHive.Abstractions.Embedding;
using IronHive.Providers.OpenAI.Embedding;
using Tiktoken;
using Tiktoken.Encodings;

namespace IronHive.Providers.OpenAI;

public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly OpenAIEmbeddingClient _client;
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAIEmbeddingProvider(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingProvider(string apiKey)
    {
        _client = new OpenAIEmbeddingClient(apiKey);
    }

    /// <inheritdoc />
    public required string ProviderName { get; init; }

    /// <inheritdoc />
    public async Task<IEnumerable<float>> EmbedAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new OpenAIEmbeddingRequest
        {
            Model = modelId,
            Input = new[] { input }
        }, cancellationToken);

        return res.Data?.FirstOrDefault()?.Embedding ??
                throw new InvalidOperationException("No embedding found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbeddingAsync(new OpenAIEmbeddingRequest
        {
            Model = modelId,
            Input = inputs,
        }, cancellationToken);

        var result = res.Data?.Select(r => new EmbeddingResult
        {
            Index = r.Index,
            Embedding = r.Embedding,
        });
        return result ?? [];
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        // OpenAI Embedding models use the cl100k_base tokenizer
        // see https://cookbook.openai.com/examples/how_to_count_tokens_with_tiktoken

        var count = _tokenizer.CountTokens(input);
        return await Task.FromResult(count);
    }
}
