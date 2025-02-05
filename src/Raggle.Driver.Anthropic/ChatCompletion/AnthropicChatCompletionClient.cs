using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using Raggle.Driver.Anthropic.ChatCompletion.Models;
using Raggle.Driver.Anthropic.Configurations;
using Raggle.Driver.Anthropic.Base;
using System.Runtime.CompilerServices;

namespace Raggle.Driver.Anthropic.ChatCompletion;

internal class AnthropicChatCompletionClient : AnthropicClientBase
{
    internal AnthropicChatCompletionClient(AnthropicConfig config) : base(config) { }

    internal AnthropicChatCompletionClient(string apiKey) : base(apiKey) { }

    internal async Task<IEnumerable<AnthropicChatCompletionModel>> GetChatCompletionModelsAsync(
        CancellationToken cancellationToken)
    {
        var models = await GetModelsAsync(cancellationToken);
        return models.Where(AnthropicModel.IsChatCompletionModel)
                     .Select(m => new AnthropicChatCompletionModel
                     {
                         Type = m.Type,
                         ID = m.ID,
                         DisplayName = m.DisplayName,
                         CreatedAt = m.CreatedAt,
                     });
    }

    internal async Task<MessagesResponse> PostMessagesAsync(
        MessagesRequest request, 
        CancellationToken cancellationToken)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(AnthropicConstants.PostMessagesPath, content, cancellationToken);
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
        using var _request = new HttpRequestMessage(HttpMethod.Post, AnthropicConstants.PostMessagesPath);
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
