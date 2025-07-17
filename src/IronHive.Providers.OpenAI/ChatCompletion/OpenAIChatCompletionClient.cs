﻿using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Runtime.CompilerServices;

namespace IronHive.Providers.OpenAI.ChatCompletion;

public class OpenAIChatCompletionClient : OpenAIClientBase
{
    public OpenAIChatCompletionClient(string apiKey) : base(apiKey) { }

    public OpenAIChatCompletionClient(OpenAIConfig config) : base(config) { }

    public async Task<ChatCompletionResponse> PostChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Client.PostAsync(OpenAIConstants.PostChatCompletionPath.RemovePreffix('/'), content, cancellationToken);
        if (!response.IsSuccessStatusCode && response.TryExtractMessage(out string error))
            throw new HttpRequestException(error);
        response.EnsureSuccessStatusCode();

        var message = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingChatCompletionAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        request.Stream = true;
        request.StreamOptions = new ChatCompletionStreamOptions { InCludeUsage = true };
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var _request = new HttpRequestMessage(HttpMethod.Post, OpenAIConstants.PostChatCompletionPath.RemovePreffix('/'));
        _request.Content = content;
        using var response = await Client.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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

                var message = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(data, JsonOptions);
                if (message != null)
                {
                    yield return message;
                }
            }

            // 특정 플랫폼(gpu_stack)의 경우
            if (line.StartsWith("error:"))
            {
                var data = line.Substring("error:".Length).Trim();
                throw new InvalidOperationException(data);
            }
        }
    }
}
