using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using IronHive.Providers.OpenAI.Payloads.Responses;
using System.Text.Json.Nodes;

namespace IronHive.Providers.OpenAI.Clients;

public class OpenAIResponsesClient : OpenAIClientBase
{
    public OpenAIResponsesClient(string apiKey) : base(apiKey) { }

    public OpenAIResponsesClient(OpenAIConfig config) : base(config) { }

    public async Task<ResponsesResponse> PostResponsesAsync(
        ResponsesRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OpenAIConstants.PostResponsesPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var raw = await response.Content.ReadFromJsonAsync<JsonObject>(_jsonOptions, cancellationToken);
        var message = JsonSerializer.Deserialize<ResponsesResponse>(raw, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        message.Raw = raw;
        return message;
    }

    public async IAsyncEnumerable<StreamingResponsesResponse> PostStreamingResponsesAsync(
        ResponsesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ResponsesStreamOptions { IncludeObfuscation = true };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostResponsesPath.RemovePrefix('/'));
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
                var message = JsonSerializer.Deserialize<StreamingResponsesResponse>(raw, _jsonOptions);
                if (message != null)
                {
                    message.Raw = raw;
                    yield return message;
                }
            }
        }
    }

    public async Task<ResponsesTokenCountResponse> PostResponsesTokenCountAsync(
        ResponsesTokenCountRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OpenAIConstants.PostResponsesTokenCountPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ResponsesTokenCountResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }
}