using IronHive.Providers.Anthropic.Payloads.Messages;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.Anthropic.Clients;

internal class AnthropicMessagesClient : AnthropicClientBase
{
    internal AnthropicMessagesClient(AnthropicConfig config) : base(config) 
    { }

    internal AnthropicMessagesClient(string apiKey) : base(apiKey) 
    { }

    internal async Task<MessagesResponse> PostMessagesAsync(
        MessagesRequest request,
        CancellationToken cancellationToken)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(AnthropicConstants.PostMessagesPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
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
        using var _request = new HttpRequestMessage(HttpMethod.Post, AnthropicConstants.PostMessagesPath.RemovePrefix('/'));
        _request.Content = content;
        using var response = await _client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            //Console.WriteLine(line);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data:"))
            {
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

    internal async Task<TokenCountResponse> PostCountTokenAsync(
        TokenCountRequest request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(AnthropicConstants.PostCountTokensPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<TokenCountResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }
}