using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using IronHive.Providers.OpenAI.Payloads.Responses;

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
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(OpenAIConstants.PostResponsesPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ResponsesResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    public async IAsyncEnumerable<StreamingResponsesResponse> PostStreamingResponsesAsync(
        ResponsesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ResponsesStreamOptions { IncludeObfuscation = true };
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostResponsesPath.RemovePrefix('/'));
        _request.Content = content;
        using var response = await Client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data:", StringComparison.Ordinal))
            {
                var data = line.Substring("data:".Length).Trim();
                if (!data.StartsWith('{') || !data.EndsWith('}'))
                    continue;

                var message = JsonSerializer.Deserialize<StreamingResponsesResponse>(data, JsonOptions);
                if (message != null)
                {
                    yield return message;
                }
            }
        }
    }

    public async Task<ResponsesTokenCountResponse> PostResponsesTokenCountAsync(
        ResponsesTokenCountRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(OpenAIConstants.PostResponsesTokenCountPath.RemovePrefix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ResponsesTokenCountResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }
}