using IronHive.Providers.Ollama.Payloads.Chat;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.Ollama.Clients;

internal class OllamaChatClient : OllamaClientBase
{
    internal OllamaChatClient(OllamaConfig? config = null) : base(config) 
    { }

    internal OllamaChatClient(string baseUrl) : base(baseUrl) 
    { }

    internal async Task<ChatResponse> PostChatAsync(
        ChatRequest request,
        CancellationToken cancellationToken)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OllamaConstants.PostChatCompletionPath, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ChatResponse>(_jsonOptions, cancellationToken).ConfigureAwait(false)
            ?? throw new HttpRequestException("Failed to deserialize response");
        return message;
    }

    internal async IAsyncEnumerable<ChatResponse> PostSteamingChatAsync(
        ChatRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        request.Stream = true;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OllamaConstants.PostChatCompletionPath);
        _request.Content = content;
        using var response = await _client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith('{') || !line.EndsWith('}'))
                continue;

            var message = JsonSerializer.Deserialize<ChatResponse>(line, _jsonOptions);
            if (message != null)
            {
                yield return message;
            }
        }
    }
}
