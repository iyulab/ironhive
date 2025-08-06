using IronHive.Abstractions.Embedding;
using IronHive.Providers.OpenAI.Embedding;
using Tiktoken;
using Tiktoken.Encodings;

namespace IronHive.Providers.OpenAI;

public class OpenAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly OpenAIEmbeddingClient _client;
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAIEmbeddingGenerator(OpenAIConfig config)
    {
        _client = new OpenAIEmbeddingClient(config);
    }

    public OpenAIEmbeddingGenerator(string apiKey)
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
            Input = [ input ]
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
        // OpenAI 임베딩 모델은 최대 300,000 토큰까지 배치 처리를 지원하므로, 나누어 배치 처리를 수행합니다.
        const int MaxTokensPerBatch = 300_000;
        var batches = await PrepareBatchListAsync(modelId, inputs, MaxTokensPerBatch, cancellationToken);

        var tasks = batches.Select(async batch =>
        {
            var inputs = batch.Select(x => x.Input).ToList();
            var indices = batch.Select(x => x.Index).ToList();

            var res = await _client.PostEmbeddingAsync(new OpenAIEmbeddingRequest
            {
                Model = modelId,
                Input = inputs
            }, cancellationToken);

            return res.Data?.Select(r => new EmbeddingResult
            {
                Index = r.Index.HasValue ? indices.ElementAt(r.Index.Value) : null,
                Embedding = r.Embedding,
            }) ?? [];
        });

        var results = (await Task.WhenAll(tasks))
            .SelectMany(r => r)
            .OrderBy(r => r.Index)
            .ToList();

        return results;
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

    /// <summary>
    /// 문자열 입력을 배치로 나누기 위한 내부 구조체입니다.
    /// </summary>
    private readonly record struct BatchItem(string Input, int Tokens, int Index);

    /// <summary>
    /// 지정한 토큰수에 따라 배치를 실행할 입력 문자열 목록을 준비합니다.
    /// </summary>
    private async Task<List<List<BatchItem>>> PrepareBatchListAsync(
        string modelId,
        IEnumerable<string> inputs,
        int splitCount,
        CancellationToken cancellationToken = default)
    {
        var items = new BatchItem[inputs.Count()];
        await Parallel.ForEachAsync(Enumerable.Range(0, inputs.Count()), cancellationToken, async (idx, ct) =>
        {
            var input = inputs.ElementAt(idx);
            var tokens = await CountTokensAsync(modelId, input, ct);
            items[idx] = new BatchItem(input, tokens, idx);
        });

        var batches = new List<List<BatchItem>>();
        var currentBatch = new List<BatchItem>();
        var currentTokenSum = 0;

        foreach (var item in items)
        {
            if (currentTokenSum + item.Tokens > splitCount && currentBatch.Count > 0)
            {
                batches.Add(currentBatch);
                currentBatch = new List<BatchItem>();
                currentTokenSum = 0;
            }

            currentBatch.Add(item);
            currentTokenSum += item.Tokens;
        }

        if (currentBatch.Count > 0)
            batches.Add(currentBatch);

        return batches;
    }

}
