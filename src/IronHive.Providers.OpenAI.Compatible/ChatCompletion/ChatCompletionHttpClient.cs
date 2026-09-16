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
    private readonly IReadOnlyDictionary<string, string>? _headers;

    public ChatCompletionHttpClient(OpenAIConfig config)
    {
        // Per request, not DefaultRequestHeaders: the client may be the consumer's own (shared, IHttpClientFactory-managed).
        _headers = ProviderRequestHeaders.Resolve(nameof(OpenAIConfig), nameof(OpenAIConfig.ApiKey), ["Authorization"], config.Headers);
        _http = config.HttpClient ?? new HttpClient(new SocketsHttpHandler
        {
            ConnectTimeout = config.ConnectTimeout
        });
        _http.BaseAddress = new Uri((string.IsNullOrWhiteSpace(config.BaseUrl)
            ? "https://api.openai.com/v1/" : config.BaseUrl).EnsureSuffix('/'));
        _http.Timeout = config.Timeout;

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        if (!string.IsNullOrWhiteSpace(config.Organization))
            _http.DefaultRequestHeaders.Add("OpenAI-Organization", config.Organization);
        if (!string.IsNullOrWhiteSpace(config.Project))
            _http.DefaultRequestHeaders.Add("OpenAI-Project", config.Project);
    }

    public void Dispose() => _http.Dispose();

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

        HttpResponseMessage response;
        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsPath) { Content = content };
            ApplyHeaders(httpRequest);
            response = await _http.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout (not the caller's token) cut the request short.
            throw new TimeoutException("The chat completion request timed out.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw await ChatCompletionExceptionDetector.DetectAsync(response, cancellationToken).ConfigureAwait(false);

            return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException("Failed to deserialize the chat completion response.");
        }
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };

        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsPath) { Content = content };
        ApplyHeaders(httpRequest);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout (not the caller's token) cut the request short.
            throw new TimeoutException("The chat completion request timed out.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw await ChatCompletionExceptionDetector.DetectAsync(response, cancellationToken).ConfigureAwait(false);

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
