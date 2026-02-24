using IronHive.Abstractions.Embedding;
using IronHive.Providers.OpenAI.Clients;
using IronHive.Providers.OpenAI.Payloads.Embedding;
using Tiktoken;
using Tiktoken.Encodings;

namespace IronHive.Providers.OpenAI;

/// <summary>
/// <para>OpenAI에서 제공하는 임베딩 모델을 사용하여 텍스트 임베딩을 생성하는 클래스입니다.</para>
/// 토큰 카운팅은 <c>cl100k_base</c> 토크나이저를 사용합니다.
/// <see href="https://cookbook.openai.com/examples/how_to_count_tokens_with_tiktoken" />을 참고하세요.
/// </summary>
public class OpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIEmbeddingClient _client;
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAIEmbeddingGenerator(string apiKey)
        : this(new OpenAIConfig { ApiKey = apiKey })
    { }

    public OpenAIEmbeddingGenerator(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual async Task<float[]> EmbedAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var request = new OpenAIEmbeddingRequest
        {
            Model = modelId,
            Input = [ input ]
        };

        var payload = OnBeforeEmbed(request);
        var res = await _client.PostEmbeddingAsync(payload, cancellationToken);
        var embedding = res.Data?.FirstOrDefault()?.Embedding?.ToArray()
            ?? throw new InvalidOperationException("No embedding found for the input.");

        return embedding;
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        // OpenAI 임베딩 모델은 최대 300,000 토큰까지 배치 처리를 지원하므로, 나누어 배치 처리를 수행합니다.
        const int MaxTokensPerBatch = 300_000;
        var tokens = await CountTokensBatchAsync(modelId, inputs, cancellationToken);
        var chunks = tokens.ChunkBy(MaxTokensPerBatch);

        var tasks = chunks.Select(async chunk =>
        {
            var inputs = chunk.Select(x => x.Text).ToList();
            var indices = chunk.Select(x => x.Index).ToList();

            var request = new OpenAIEmbeddingRequest
            {
                Model = modelId,
                Input = inputs
            };

            var payload = OnBeforeEmbed(request);
            var res = await _client.PostEmbeddingAsync(payload, cancellationToken);

            return res.Data?.Select(r => new EmbeddingResult
            {
                Index = r.Index.HasValue ? indices.ElementAt(r.Index.Value) : null,
                Embedding = r.Embedding,
            }) ?? [];
        });

        return (await Task.WhenAll(tasks))
            .SelectMany(r => r)
            .OrderBy(r => r.Index);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountTokensAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var count = _tokenizer.CountTokens(input);
        return await Task.FromResult(count);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId, 
        IEnumerable<string> inputs, 
        CancellationToken cancellationToken)
    {
        var items = new EmbeddingTokens[inputs.Count()];

        await Parallel.ForEachAsync(Enumerable.Range(0, inputs.Count()), cancellationToken, async (idx, ct) =>
        {
            await Task.Run(() =>
            {
                var text = inputs.ElementAt(idx);
                var tokenCount = _tokenizer.CountTokens(text);
                items[idx] = new EmbeddingTokens
                {
                    Index = idx,
                    Text = text,
                    TokenCount = tokenCount
                };
            }, ct);
        });

        // 생성자에서 인덱스 순으로 정렬을 보장합니다.
        return [.. items];
    }

    /// <summary>
    /// 임베딩 요청을 보내기 전에 처리할 수 있는 가상 메서드입니다.
    /// </summary>
    protected virtual OpenAIEmbeddingRequest OnBeforeEmbed(
        OpenAIEmbeddingRequest request) 
        => request;
}