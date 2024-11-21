using Raggle.Driver.Ollama.Base;
using Raggle.Driver.Ollama.ChatCompletion.Models;
using Raggle.Driver.Ollama.Configurations;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Raggle.Driver.Ollama.ChatCompletion;

internal class OllamaChatCompletionClient : OllamaClientBase
{
    internal OllamaChatCompletionClient(OllamaConfig? config = null) : base(config) { }

    internal OllamaChatCompletionClient(string endPoint) : base(endPoint) { }

    internal async Task<IEnumerable<OllamaModel>> GetChatModelsAsync(
        CancellationToken cancellationToken)
    {
        // Ollama does not have a information about model categories
        return await GetModelsAsync(cancellationToken);
    }

    internal async Task<ChatResponse> PostChatAsync(
        ChatRequest request, 
        CancellationToken cancellationToken)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(OllamaConstants.PostChatCompletionPath, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ChatResponse>(_jsonOptions, cancellationToken)
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
        var response = await _client.PostAsync(OllamaConstants.PostChatCompletionPath, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync();
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
