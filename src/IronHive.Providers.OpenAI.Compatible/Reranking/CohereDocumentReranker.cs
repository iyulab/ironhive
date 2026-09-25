using IronHive.Abstractions.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using IronHive.Abstractions.Reranking;
using IronHive.Providers.OpenAI;

namespace IronHive.Providers.OpenAI.Compatible.Reranking;

/// <summary>
/// Reranker for a generic endpoint that exposes a Cohere-shaped <c>POST /rerank</c>
/// (<see href="https://docs.cohere.com/reference/rerank"/>) — the shape self-hosted rerank servers
/// such as Infinity (explicitly a "locally deployed Cohere rerank API"), vLLM's
/// <c>/rerank</c>/<c>/v1/rerank</c>/<c>/v2/rerank</c> (documented as Cohere/Jina-compatible), and
/// GPUStack's <c>/v1/rerank</c> (Jina-compatible, a superset of Cohere's shape) implement. Not
/// every self-hosted rerank server follows this shape — HuggingFace's Text Embeddings Inference
/// (TEI), for one, uses its own distinct request fields
/// (<c>texts</c>/<c>raw_scores</c>/<c>return_text</c> rather than Cohere's
/// <c>documents</c>/<c>model</c>/<c>top_n</c>) and is not compatible with this client.
/// <para>
/// Takes an already-resolved <see cref="OpenAIConfig"/> (<c>BaseUrl</c>/<c>ApiKey</c>/
/// <c>ConnectTimeout</c>/<c>Timeout</c>/<c>HttpClient</c>) rather than
/// <see cref="OpenAICompatibleConfig"/> directly, mirroring <c>ChatCompletionMessageGenerator</c> —
/// callers resolve their own config to an <see cref="OpenAIConfig"/> first (via
/// <see cref="OpenAICompatibleConfig.ToOpenAI"/> or a provider-specific equivalent) because the
/// rerank path is not always the same as the chat/embeddings path on the same server (GPUStack
/// serves chat/embeddings at <c>/v1-openai/</c> but rerank at <c>/v1/</c>).
/// </para>
/// </summary>
public class CohereDocumentReranker : IDocumentReranker
{
    private const string RerankPath = "rerank";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly ProviderHttpClient _http;

    /// <remarks>An injected <see cref="OpenAIConfig.HttpClient"/> is used as given and never disposed — see <see cref="ProviderHttpClient"/>.</remarks>
    public CohereDocumentReranker(OpenAIConfig config)
        => _http = new ProviderHttpClient(config, RerankPath, defaultBaseUrl: null, sendAccountHeaders: false);

    /// <inheritdoc />
    public void Dispose()
    {
        _http.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RerankResult>> RerankAsync(
        string modelId,
        string query,
        IEnumerable<string> documents,
        int? topN = null,
        CancellationToken cancellationToken = default)
    {
        var request = new CohereRerankRequest
        {
            Model = modelId,
            Query = query,
            Documents = documents.ToList(),
            TopN = topN,
        };

        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = _http.CreatePost(content);
        using var timeout = _http.CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;

        CohereRerankResponse result;
        try
        {
            using var response = await _http.Http.SendAsync(httpRequest, token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                throw new HttpRequestException($"Rerank request failed with status {(int)response.StatusCode}: {body}");
            }

            result = await response.Content.ReadFromJsonAsync<CohereRerankResponse>(JsonOptions, token).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Failed to deserialize the rerank response.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The configured timeout, or an injected client's own HttpClient.Timeout — not the caller's token.
            throw new TimeoutException("The rerank request timed out.", ex);
        }

        return result.Results.Select(r => new RerankResult
        {
            Index = r.Index,
            Score = r.RelevanceScore,
        });
    }
}
