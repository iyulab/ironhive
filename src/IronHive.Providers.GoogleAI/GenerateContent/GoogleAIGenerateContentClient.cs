using IronHive.Abstractions.Messages;
using IronHive.Providers.GoogleAI.GenerateContent.Models;
using IronHive.Providers.GoogleAI.Share;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace IronHive.Providers.GoogleAI.GenerateContent;

internal class GoogleAIGenerateContentClient : GoogleAIClientBase
{
    public GoogleAIGenerateContentClient(GoogleAIConfig config) : base(config)
    { }

    public GoogleAIGenerateContentClient(string apiKey) : base(apiKey)
    { }

    public async Task<GenerateContentResponse> GenerateContentAsync(
        string modelId,
        GenerateContentRequest request, 
        CancellationToken cancellationToken = default)
    {
        var path = string.Format(GoogleAIConstants.PostGenerateContentPath, modelId).RemovePreffix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(path, content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var jsonDoc = await response.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken);
        Console.WriteLine(jsonDoc?.RootElement.ToString());

        return JsonSerializer.Deserialize<GenerateContentResponse>(jsonDoc?.RootElement.ToString() ?? "",
            _jsonOptions) ?? throw new InvalidOperationException("Failed to deserialize response.");

        //var res = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(_jsonOptions, cancellationToken)
        //    ?? throw new InvalidOperationException("Failed to deserialize response.");
        //return res;
    }

    public async IAsyncEnumerable<GenerateContentResponse> GenerateStreamContentAsync(
        string modelId,
        GenerateContentRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var path = string.Format(GoogleAIConstants.PostStreamGenerateContentPath, modelId).RemovePreffix('/');
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var _request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
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
            Console.WriteLine(line);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("data:"))
            {
                var data = line.Substring("data:".Length).Trim();
                if (!data.StartsWith('{') || !data.EndsWith('}'))
                    continue;

                var message = JsonSerializer.Deserialize<GenerateContentResponse>(data, _jsonOptions);
                if (message != null)
                {
                    yield return message;
                }
            }
        }
    }
}