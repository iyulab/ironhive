using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text;
using System.Runtime.CompilerServices;
using IronHive.Providers.OpenAI.Payloads.ChatCompletion;

namespace IronHive.Providers.OpenAI.Clients;

public class OpenAIChatCompletionClient : OpenAIClientBase
{
    public OpenAIChatCompletionClient(string apiKey) : base(apiKey) { }

    public OpenAIChatCompletionClient(OpenAIConfig config) : base(config) { }

    public async Task<ChatCompletionResponse> PostChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OpenAIConstants.PostChatCompletionPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<JsonObject>(_jsonOptions, cancellationToken);
        var message = JsonSerializer.Deserialize<ChatCompletionResponse>(raw, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        message.Raw = raw;
        return message;
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { IncludeUsage = true };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostChatCompletionPath.RemovePrefix('/'));
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

                var raw = JsonSerializer.Deserialize<JsonObject>(data, _jsonOptions);
                var message = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(raw, _jsonOptions);
                if (message != null)
                {
                    message.Raw = raw;
                    yield return message;
                }
            }
        }
    }
}
