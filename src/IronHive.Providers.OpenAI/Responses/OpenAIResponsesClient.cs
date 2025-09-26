using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using IronHive.Providers.OpenAI.Responses.Models;
using IronHive.Providers.OpenAI.Share;

namespace IronHive.Providers.OpenAI.Responses;

internal class OpenAIResponsesClient : OpenAIClientBase
{
    internal OpenAIResponsesClient(string apiKey) : base(apiKey) { }

    internal OpenAIResponsesClient(OpenAIConfig config) : base(config) { }

    internal async Task<ResponsesResponse> PostResponsesAsync(
        ResponsesRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OpenAIConstants.PostResponsesPath.RemovePreffix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ResponsesResponse>(_jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    internal async IAsyncEnumerable<StreamingResponsesResponse> PostStreamingResponsesAsync(
        ResponsesRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ResponsesStreamOptions { IncludeObfuscation = true };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostResponsesPath.RemovePreffix('/'));
        _request.Content = content;
        using var response = await _client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            //Console.WriteLine(line);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data:"))
            {
                var data = line.Substring("data:".Length).Trim();
                if (!data.StartsWith('{') || !data.EndsWith('}'))
                    continue;

                var message = JsonSerializer.Deserialize<StreamingResponsesResponse>(data, _jsonOptions);
                if (message != null)
                {
                    yield return message;
                }
            }
        }
    }
}
