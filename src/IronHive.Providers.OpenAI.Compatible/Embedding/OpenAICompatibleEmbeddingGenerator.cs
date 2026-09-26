using System.Net.Http.Json;
using System.Text.Json.Nodes;
using IronHive.Abstractions.Embedding;
using IronHive.Providers.OpenAI.Compatible.ChatCompletion;
using Tiktoken;
using Tiktoken.Encodings;

namespace IronHive.Providers.OpenAI.Compatible.Embedding;

/// <summary>
/// Raw HTTP <c>POST /embeddings</c> client for OpenAI-compatible servers (vLLM, llama.cpp server, GPUStack, Ollama, ...).
/// Raw rather than the OpenAI SDK's <c>EmbeddingClient</c> for the reason the chat path is: the SDK's request model has
/// no slot for server extensions, so <see cref="EmbeddingRequestOptions.ExtraBody"/> could not reach the server.
/// </summary>
/// <remarks>
/// Token counts from <see cref="CountTokensAsync"/> are a cl100k <b>estimate</b> (a compatible server's tokenizer is
/// its own), used only to split a large batch. <see cref="EmbeddingResponse.InputTokens"/> is the server's reported
/// <c>usage</c>, never the estimate. An injected <see cref="OpenAIConfig.HttpClient"/> is used as given and never
/// disposed.
/// </remarks>
public class OpenAICompatibleEmbeddingGenerator : IEmbeddingGenerator
{
    private const string EmbeddingsPath = "embeddings";

    // The token budget one request carries, by the estimate (OpenAI's documented per-request limit).
    private const int MaxTokensPerBatch = 300_000;

    private readonly ProviderHttpClient _http;
    private readonly Encoder _tokenizer = new(new Cl100KBase());

    public OpenAICompatibleEmbeddingGenerator(OpenAIConfig config)
        => _http = new ProviderHttpClient(config, EmbeddingsPath, "https://api.openai.com/v1/", sendAccountHeaders: true);

    /// <inheritdoc />
    public void Dispose()
    {
        _http.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<float[]> EmbedAsync(string modelId, string input, CancellationToken cancellationToken = default)
        => EmbedAsync(modelId, input, options: null, cancellationToken);

    /// <inheritdoc />
    public async Task<float[]> EmbedAsync(
        string modelId,
        string input,
        EmbeddingRequestOptions? options,
        CancellationToken cancellationToken = default)
    {
        var response = await EmbedBatchAsync(modelId, [input], options, cancellationToken).ConfigureAwait(false);
        return (response.Results.Count > 0 ? response.Results[0].Embedding : null)
            ?? throw new InvalidOperationException("The embedding response carried no vector.");
    }

    /// <inheritdoc />
    public Task<EmbeddingResponse> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
        => EmbedBatchAsync(modelId, inputs, options: null, cancellationToken);

    /// <inheritdoc />
    public async Task<EmbeddingResponse> EmbedBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        EmbeddingRequestOptions? options,
        CancellationToken cancellationToken = default)
    {
        var tokens = await CountTokensBatchAsync(modelId, inputs, cancellationToken).ConfigureAwait(false);
        var parts = await Task.WhenAll(tokens.ChunkBy(MaxTokensPerBatch).Select(chunk =>
            PostAsync(modelId, chunk.ToList(), options?.ExtraBody, cancellationToken))).ConfigureAwait(false);

        return new EmbeddingResponse
        {
            Results = parts.SelectMany(p => p.Results).OrderBy(r => r.Index).ToList(),
            // Summed only when every request reported: a partial sum would read as the whole call.
            InputTokens = parts.All(p => p.InputTokens.HasValue) ? parts.Sum(p => p.InputTokens!.Value) : null,
            Model = parts.Select(p => p.Model).FirstOrDefault(m => m != null),
        };
    }

    private async Task<(List<EmbeddingResult> Results, int? InputTokens, string? Model)> PostAsync(
        string modelId,
        List<EmbeddingTokens> chunk,
        JsonObject? extraBody,
        CancellationToken cancellationToken)
    {
        var body = BuildRequestBody(modelId, chunk.Select(c => c.Text), extraBody);
        using var content = JsonContent.Create(body);
        using var httpRequest = _http.CreatePost(content);
        using var timeout = _http.CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;

        JsonObject root;
        try
        {
            using var response = await _http.Http.SendAsync(httpRequest, token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw await ChatCompletionExceptionDetector.DetectAsync(response, token).ConfigureAwait(false);

            var json = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
            root = JsonNode.Parse(json) as JsonObject
                ?? throw new InvalidOperationException("The embedding response is not a JSON object.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The configured timeout, or an injected client's own HttpClient.Timeout, not the caller's token.
            throw new TimeoutException("The embedding request timed out.", ex);
        }

        return ParseResponse(root, chunk);
    }

    /// <summary>The request body: the typed fields, then the caller's <c>ExtraBody</c> merged over them.</summary>
    internal static JsonObject BuildRequestBody(string modelId, IEnumerable<string> inputs, JsonObject? extraBody)
    {
        var body = new JsonObject
        {
            ["model"] = modelId,
            ["input"] = new JsonArray(inputs.Select(i => (JsonNode?)JsonValue.Create(i)).ToArray()),
        };
        if (extraBody is { Count: > 0 })
            JsonObjectMerge.DeepMerge(body, extraBody);
        return body;
    }

    /// <summary>Maps <c>data[].index</c> back to the caller's positions and reads <c>usage</c> and <c>model</c>.</summary>
    internal static (List<EmbeddingResult> Results, int? InputTokens, string? Model) ParseResponse(
        JsonObject root, IReadOnlyList<EmbeddingTokens> chunk)
    {
        var data = root["data"] as JsonArray
            ?? throw new InvalidOperationException("The embedding response has no 'data' array.");

        var results = new List<EmbeddingResult>(data.Count);
        for (var i = 0; i < data.Count; i++)
        {
            var item = data[i] as JsonObject
                ?? throw new InvalidOperationException("An embedding response item is not an object.");
            // "index" is the position in this request's input; a server that omits it answers in order.
            var position = item["index"]?.GetValue<int>() ?? i;
            if (position < 0 || position >= chunk.Count)
                throw new InvalidOperationException(
                    $"The embedding response index {position} is outside the {chunk.Count} input(s) sent.");

            var vector = item["embedding"] as JsonArray
                ?? throw new InvalidOperationException(
                    "An embedding response item has no float 'embedding' array (a base64 encoding_format is not read).");
            results.Add(new EmbeddingResult
            {
                Index = chunk[position].Index,
                Embedding = vector.Select(v => v!.GetValue<float>()).ToArray(),
            });
        }

        // A server that omits "usage", or reports zero, has not reported.
        var reported = root["usage"]?["prompt_tokens"]?.GetValue<int>() is int n && n > 0 ? n : (int?)null;
        var model = root["model"]?.GetValue<string>() is { Length: > 0 } m ? m : null;
        return (results, reported, model);
    }

    /// <inheritdoc />
    public Task<int> CountTokensAsync(string modelId, string input, CancellationToken cancellationToken = default)
        => Task.FromResult(_tokenizer.CountTokens(input));

    /// <inheritdoc />
    public Task<IEnumerable<EmbeddingTokens>> CountTokensBatchAsync(
        string modelId,
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<EmbeddingTokens> items = inputs.Select((text, index) => new EmbeddingTokens
        {
            Index = index,
            Text = text,
            TokenCount = _tokenizer.CountTokens(text),
        }).ToList();
        return Task.FromResult(items);
    }
}
