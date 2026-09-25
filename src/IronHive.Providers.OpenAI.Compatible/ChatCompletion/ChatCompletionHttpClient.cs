using IronHive.Abstractions.Http;
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
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        using var content = JsonContent.Create(request, options: JsonOptions);

        using var timeout = _http.CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;
        try
        {
            using var httpRequest = _http.CreatePost(content);
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
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };

        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = _http.CreatePost(content);
        using var timeout = _http.CreateTimeoutSource(cancellationToken);
        var token = timeout?.Token ?? cancellationToken;

        HttpResponseMessage response;
        try
        {
            response = await _http.Http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
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
