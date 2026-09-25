using IronHive.Abstractions.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronHive.Providers.OpenAI.Compatible.ChatCompletion;

/// <summary>
/// Raw HTTP <c>POST /chat/completions</c> client. Bypassing the OpenAI SDK here (unlike the first-party
/// <see cref="OpenAIMessageGenerator"/>, which uses it for the Responses API) is deliberate: the SDK's
/// strongly-typed response models have no slot for vendor extensions such as <c>reasoning_content</c>
/// (emitted by Ollama/vLLM-style reasoning models over Chat Completions), and its streaming reader exposes
/// no raw-JSON escape hatch to recover them either — see openai/openai-dotnet#813. Parsing the JSON directly
/// via <see cref="ExtraBodyJsonConverterFactory"/> keeps that data reachable.
/// </summary>
internal sealed class ChatCompletionHttpClient : IDisposable
{
    private const string ChatCompletionsPath = "chat/completions";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowOutOfOrderMetadataProperties = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
            new ExtraBodyJsonConverterFactory(),
        },
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttp;
    private readonly Uri _endpoint;
    private readonly TimeSpan _timeout;
    private readonly string? _apiKey;
    private readonly string? _organization;
    private readonly string? _project;
    private readonly IReadOnlyDictionary<string, string>? _headers;

    /// <remarks>
    /// An injected <see cref="OpenAIConfig.HttpClient"/> is the consumer's — typically from <c>IHttpClientFactory</c>
    /// and shared — so it is used as given: nothing is set on it (a client that has sent a request refuses changes)
    /// and it is never disposed. The endpoint, the credentials and <see cref="OpenAIConfig.Timeout"/> travel with each
    /// request instead, which is also why two generators can share one client.
    /// </remarks>
    public ChatCompletionHttpClient(OpenAIConfig config)
    {
        _headers = ProviderRequestHeaders.Resolve(nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), ["Authorization"], config.Headers);
        _ownsHttp = config.HttpClient is null;
        _http = config.HttpClient ?? new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = config.ConnectTimeout
        })
        {
            // The request timeout is applied per request below, the same way for an owned and an injected client.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan,
        };
        var baseUrl = new Uri((string.IsNullOrWhiteSpace(config.BaseUrl)
            ? "https://api.openai.com/v1/" : config.BaseUrl).EnsureSuffix('/'));
        _endpoint = new Uri(baseUrl, ChatCompletionsPath);
        _timeout = config.Timeout;
        _apiKey = string.IsNullOrWhiteSpace(config.ApiKey) ? null : config.ApiKey;
        _organization = string.IsNullOrWhiteSpace(config.Organization) ? null : config.Organization;
        _project = string.IsNullOrWhiteSpace(config.Project) ? null : config.Project;
    }

    public void Dispose()
    {
        if (_ownsHttp)
            _http.Dispose();
    }

    private HttpRequestMessage CreateRequest(HttpContent content)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _endpoint) { Content = content };
        if (_apiKey != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        if (_organization != null)
            request.Headers.Add("OpenAI-Organization", _organization);
        if (_project != null)
            request.Headers.Add("OpenAI-Project", _project);
        ApplyHeaders(request);
        return request;
    }

    /// <summary>
    /// A token that fires at <see cref="OpenAIConfig.Timeout"/> as well as on the caller's token. A cancellation the
    /// caller did not request is therefore the timeout, which is how the catch blocks below tell the two apart.
    /// </summary>
    private CancellationTokenSource? CreateTimeoutSource(CancellationToken cancellationToken)
    {
        if (_timeout == System.Threading.Timeout.InfiniteTimeSpan)
            return null;
        var source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        source.CancelAfter(_timeout);
        return source;
    }

    /// <summary>A configured header replaces any default of the same name; the credential is never among them.</summary>
    private void ApplyHeaders(HttpRequestMessage request)
    {
        if (_headers is null)
            return;
        foreach (var (name, value) in _headers)
        {
            request.Headers.Remove(name);
            request.Headers.TryAddWithoutValidation(name, value);
        }
    }

    public async Task<ChatCompletionResponse> PostAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        using var content = JsonContent.Create(request, options: JsonOptions);

        using var timeout = CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;
        try
        {
            using var httpRequest = CreateRequest(content);
            using var response = await _http.SendAsync(httpRequest, token).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                throw await ChatCompletionExceptionDetector.DetectAsync(response, token).ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, token).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Failed to deserialize the chat completion response.");
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // The configured timeout, or an injected client's own HttpClient.Timeout — not the caller's token.
            throw new TimeoutException("The chat completion request timed out.", ex);
        }
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };

        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = CreateRequest(content);
        using var timeout = CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout (not the caller's token) cut the request short.
            throw new TimeoutException("The chat completion request timed out.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw await ChatCompletionExceptionDetector.DetectAsync(response, token).ConfigureAwait(false);

            // The timeout bounds the wait for the response to start, as HttpClient.Timeout does with ResponseHeadersRead;
            // a long generation that is streaming is not cut off by it.
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            string? line;
            while (true)
            {
                try
                {
                    line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException("The chat completion request timed out.", ex);
                }

                if (line is null)
                    yield break;

                // GPUStack emits mid-stream errors as a bare "error: <message>" line instead of an HTTP error.
                if (line.StartsWith("error:", StringComparison.Ordinal))
                {
                    var errorMessage = line["error:".Length..].Trim();
                    throw ChatCompletionExceptionDetector.Detect(errorMessage);
                }

                if (!line.StartsWith("data:", StringComparison.Ordinal))
                    continue;

                var data = line["data:".Length..].Trim();
                if (data is "[DONE]" || data.Length == 0)
                    continue;

                var chunk = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(data, JsonOptions);
                if (chunk != null)
                    yield return chunk;
            }
        }
    }
}
