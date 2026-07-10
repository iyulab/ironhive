using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
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

    public ChatCompletionHttpClient(OpenAIConfig config)
    {
        _http = config.HttpClient ?? new HttpClient();
        _http.BaseAddress = new Uri((string.IsNullOrWhiteSpace(config.BaseUrl)
            ? "https://api.openai.com/v1/" : config.BaseUrl).EnsureSuffix('/'));
        _http.Timeout = config.TimeOut;

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
        if (!string.IsNullOrWhiteSpace(config.Organization))
            _http.DefaultRequestHeaders.Add("OpenAI-Organization", config.Organization);
        if (!string.IsNullOrWhiteSpace(config.Project))
            _http.DefaultRequestHeaders.Add("OpenAI-Project", config.Project);
    }

    public void Dispose() => _http.Dispose();

    public async Task<ChatCompletionResponse> PostAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        using var content = JsonContent.Create(request, options: JsonOptions);
        using var response = await _http.PostAsync(ChatCompletionsPath, content, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw await CreateErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Failed to deserialize the chat completion response.");
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };

        using var content = JsonContent.Create(request, options: JsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ChatCompletionsPath) { Content = content };
        using var response = await _http.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw await CreateErrorAsync(response, cancellationToken).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            // GPUStack emits mid-stream errors as a bare "error: <message>" line instead of an HTTP error.
            if (line.StartsWith("error:", StringComparison.Ordinal))
            {
                var errorMessage = line["error:".Length..].Trim();
                throw ChatCompletionExceptionDetector.Detect(errorMessage) as Exception
                    ?? new HttpRequestException(errorMessage);
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

    /// <summary>Builds the exception for a failed response: recursively searches the error body for a
    /// "message" property (falling back to the raw status line when the body isn't the expected
    /// <c>{"error": {"message": "..."}}</c> shape), then normalizes context-window overflow errors
    /// to <see cref="IronHive.Abstractions.Exceptions.ContextOverflowException"/>.</summary>
    private static async Task<Exception> CreateErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        JsonNode? json = null;
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            json = JsonNode.Parse(body);
        }
        catch (JsonException)
        { }

        var message = (json != null ? FindMessage(json) : null) is { Length: > 0 } found
            ? found
            : $"Chat completion request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).";

        return ChatCompletionExceptionDetector.Detect(message, json) as Exception
            ?? new HttpRequestException(message);
    }

    private static string? FindMessage(JsonNode? node)
    {
        if (node is JsonObject obj)
        {
            foreach (var kvp in obj)
            {
                if (kvp.Key.Equals("message", StringComparison.OrdinalIgnoreCase) && kvp.Value != null)
                    return kvp.Value.ToString();
                if (FindMessage(kvp.Value) is { } found)
                    return found;
            }
        }
        else if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (FindMessage(item) is { } found)
                    return found;
            }
        }
        return null;
    }
}
