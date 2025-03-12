using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using Raggle.Connectors.Anthropic.Configurations;
using System.Runtime.CompilerServices;
using Raggle.Connectors.Anthropic.Base;

namespace Raggle.Connectors.Anthropic.ChatCompletion;

internal class AnthropicChatCompletionClient : AnthropicClientBase
{
    internal AnthropicChatCompletionClient(AnthropicConfig config) : base(config) { }

    internal AnthropicChatCompletionClient(string apiKey) : base(apiKey) { }

    internal async Task<MessagesResponse> PostMessagesAsync(
        MessagesRequest request,
        CancellationToken cancellationToken)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(AnthropicConstants.PostMessagesPath.RemovePreffix('/'), content, cancellationToken);
        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadFromJsonAsync<MessagesResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    internal async IAsyncEnumerable<StreamingMessagesResponse> PostStreamingMessagesAsync(
        MessagesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        request.Stream = true;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, AnthropicConstants.PostMessagesPath.RemovePreffix('/'));
        _request.Content = content;
        using var response = await _client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data"))
                continue;
            Console.WriteLine(line);

            var data = line.Substring("data:".Length).Trim();
            if (!data.StartsWith('{') || !data.EndsWith('}'))
                continue;

            var message = JsonSerializer.Deserialize<StreamingMessagesResponse>(data, _jsonOptions);
            if (message != null)
            {
                yield return message;
            }
        }
    }

}
