using IronHive.Abstractions.Embedding;
using IronHive.Providers.GoogleAI.Clients;
using IronHive.Providers.GoogleAI.Payloads;
using IronHive.Providers.GoogleAI.Payloads.EmbedContent;
using IronHive.Providers.GoogleAI.Payloads.Tokens;

namespace IronHive.Providers.GoogleAI;

/// <inheritdoc />
public class GoogleAIEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly GoogleAIEmbedContentClient _client;
    private readonly GoogleAITokensClient _tokenizer;

    public GoogleAIEmbeddingGenerator(string apiKey)
        : this(new GoogleAIConfig { ApiKey = apiKey })
    { }

    public GoogleAIEmbeddingGenerator(GoogleAIConfig config)
    {
        _client = new GoogleAIEmbedContentClient(config);
        _tokenizer = new GoogleAITokensClient(config);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
        _tokenizer.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostEmbedContentAsync(new EmbedContentRequest
        {
            Model = modelId,
            Content = new Content
            {
                Parts = [ new ContentPart { Text = input } ]
            }
        }, cancellationToken);
        
        return res.Embedding.Values;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingResult>> EmbedBatchAsync(
        string modelId, 
        IEnumerable<string> inputs, 
        CancellationToken cancellationToken = default)
    {
        var res = await _client.PostBatchEmbedContentAsync(new BatchEmbedContentRequest
        {
            Requests = inputs.Select(input => new EmbedContentRequest
            {
                Model = modelId,
                Content = new Content
                {
                    Parts = [ new ContentPart { Text = input } ]
                }
            }).ToArray()
        }, cancellationToken);

        return res.Embeddings.Select((e, i) => new EmbeddingResult
        {
            Index = i,
            Embedding = e.Values
        });
    }

    /// <inheritdoc />
    public async Task<int> CountTokensAsync(
        string modelId, 
        string input, 
        CancellationToken cancellationToken = default)
    {
        var res = await _tokenizer.PostCountTokensAsync(new CountTokensRequest
        {
            Model = modelId,
            Contents =
            [
                new Content
                {
                    Parts = [ new ContentPart { Text = input } ]
                }
            ]
        }, cancellationToken);

        return res.TotalTokens 
            ?? throw new InvalidOperationException("No token count found for the input.");
    }

    /// <inheritdoc />
    public async Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId, 
        IEnumerable<string> inputs, 
        CancellationToken cancellationToken = default)
    {
        var res = await _tokenizer.PostCountTokensAsync(new CountTokensRequest
        {
            Model = modelId,
            Contents = inputs.Select(input => new Content
            {
                Parts = [ new ContentPart { Text = input } ]
            }).ToArray()
        }, cancellationToken);

        return res.PromptTokensDetails?.Select((d, i) => new EmbeddingTokens
        {
            Index = i,
            Text = inputs.ElementAt(i),
            TokenCount = d.TokenCount 
                ?? throw new InvalidOperationException($"No token count found for the input[{i}].")
        }) ?? [];
    }
}