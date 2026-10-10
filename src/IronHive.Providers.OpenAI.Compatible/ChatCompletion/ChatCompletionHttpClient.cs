using IronHive.Abstractions.Http;
using System.Text.Json.Nodes;
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

    private readonly ProviderHttpClient _http;

    /// <remarks>An injected <see cref="OpenAIConfig.HttpClient"/> is used as given and never disposed — see <see cref="ProviderHttpClient"/>.</remarks>
    public ChatCompletionHttpClient(OpenAIConfig config)
        => _http = new ProviderHttpClient(config, ChatCompletionsPath, "https://api.openai.com/v1/", sendAccountHeaders: true);

    public void Dispose() => _http.Dispose();

    public async Task<ChatCompletionResponse> PostAsync(
        ChatCompletionRequest request,
        IDictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        using var content = JsonContent.Create(request, options: JsonOptions);

        using var timeout = _http.CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;
        try
        {
            using var httpRequest = _http.CreatePost(content, headers);
            using var response = await _http.Http.SendAsync(httpRequest, token).ConfigureAwait(false);

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
        IDictionary<string, string>? headers = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };

        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = _http.CreatePost(content, headers);
        // OpenAIConfig.Timeout bounds the wait for the response to start, as HttpClient.Timeout does with
        // ResponseHeadersRead; a long generation that is streaming is not cut off by it. OpenAIConfig.StreamIdleTimeout
        // bounds every silence: that same wait, then each read of the body.
        using var timeout = _http.CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;

        HttpResponseMessage response;
        using (var wait = _http.CreateStreamWaitSource(token))
        {
            try
            {
                response = await _http.Http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, wait?.Token ?? token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // The configured timeout, an injected client's own HttpClient.Timeout, or the idle budget — not the caller.
                throw wait is { IsCancellationRequested: true } && timeout is not { IsCancellationRequested: true }
                    ? ProviderStreams.IdleTimeout(_http.StreamIdleTimeout, ex)
                    : new TimeoutException("The chat completion request timed out.", ex);
            }
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
                throw await ChatCompletionExceptionDetector.DetectAsync(response, token).ConfigureAwait(false);

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            string? line;
            while (true)
            {
                using (var wait = _http.CreateStreamWaitSource(cancellationToken))
                {
                    try
                    {
                        // Any line counts as the stream being alive, an SSE comment or keep-alive included.
                        line = await reader.ReadLineAsync(wait?.Token ?? cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (wait is { IsCancellationRequested: true } && !cancellationToken.IsCancellationRequested)
                    {
                        throw ProviderStreams.IdleTimeout(_http.StreamIdleTimeout, ex);
                    }
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

                // A server that fails after the 200 sends the error as a data line of its own (OpenAI, vLLM, LiteLLM and most
                // gateways): it has no choices, so read as a chunk it would vanish and the stream would look complete.
                if (TryReadStreamError(data) is { } streamError)
                    throw streamError;

                var chunk = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(data, JsonOptions);
                if (chunk != null)
                    yield return chunk;
            }
        }
    }

    /// <summary>The exception for a <c>data:</c> line whose body is an error object (<c>{"error": {"message", "type",
    /// "code"}}</c>, or <c>{"error": "…"}</c>), or null for an ordinary chunk.</summary>
    internal static Exception? TryReadStreamError(string data)
    {
        if (!data.Contains("\"error\"", StringComparison.Ordinal))
            return null;

        JsonNode? root;
        try
        {
            root = JsonNode.Parse(data);
        }
        catch (JsonException)
        {
            return null;
        }

        if (root is not JsonObject obj || !obj.TryGetPropertyValue("error", out var error) || error is null)
            return null;

        if (error is JsonValue value && value.TryGetValue<string>(out var text))
            return ChatCompletionExceptionDetector.Detect(text);

        if (error is not JsonObject body)
            return null;

        var message = body["message"] is JsonValue m && m.TryGetValue<string>(out var msg) && msg.Length > 0
            ? msg
            : body.ToJsonString();
        return ChatCompletionExceptionDetector.Detect(message, ScalarText(body["type"]), ScalarText(body["code"]));
    }

    // A code may be a string ("rate_limit_exceeded") or a number (vLLM sends the HTTP status the error would have had).
    private static string? ScalarText(JsonNode? node)
        => node is JsonValue value
            ? value.TryGetValue<string>(out var text) ? text : value.ToJsonString()
            : null;
}
