using IronHive.Abstractions.Embedding;
using IronHive.Providers.Ollama.Clients;
using IronHive.Providers.Ollama.Payloads.Embedding;
using Microsoft.ML.Tokenizers;

namespace IronHive.Providers.Ollama;

/// <inheritdoc />
public class OllamaEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OllamaEmbeddingClient _client;

    // Llama 모델용 토크나이저 (Ollama의 대부분 모델이 Llama 기반)
    private static readonly Lazy<Tokenizer> LazyTokenizer = new(
        () => TiktokenTokenizer.CreateForModel("gpt-4o"));

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
    /// <remarks>
    /// Ollama API는 토큰 카운트를 직접 지원하지 않으므로,
    /// GPT-4o 토크나이저(o200k_base)를 사용하여 근사치를 반환합니다.
    /// 실제 모델의 토큰 수와 다를 수 있습니다.
    /// </remarks>
    public Task<int> CountTokensAsync(
        string modelId,
        string input,
        CancellationToken cancellationToken = default)
    {
        var tokenizer = LazyTokenizer.Value;
        var count = tokenizer.CountTokens(input);
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Ollama API는 토큰 카운트를 직접 지원하지 않으므로,
    /// GPT-4o 토크나이저(o200k_base)를 사용하여 근사치를 반환합니다.
    /// 실제 모델의 토큰 수와 다를 수 있습니다.
    /// </remarks>
    public Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var tokenizer = LazyTokenizer.Value;
        var results = inputs.Select((input, index) => new EmbeddingTokens
        {
            Index = index,
            Text = input,
            TokenCount = tokenizer.CountTokens(input)
        });
        return Task.FromResult(results);
    }
}
