using Raggle.Engines.Anthropic.Configurations;
using System.Text.Json;
using System.Text;
using System.Net.Http.Json;
using Raggle.Engines.Anthropic.Abstractions;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class AnthropicChatCompletionClient : AnthropicClientBase
{
    public AnthropicChatCompletionClient(AnthropicConfig config) : base(config) { }

    public AnthropicChatCompletionClient(string apiKey) : base(apiKey) { }

    /// <summary>
    /// Anthropic does not provide a way to list available models.
    /// so this list is hardcoded models, writed at 2024-09-02.
    /// </summary>
    public IEnumerable<AnthropicChatCompletionModel> GetChatCompletionModels()
    {
        var models = GetModels();
        return models.Select(m => new AnthropicChatCompletionModel
        {
            ModelId = m.ModelId,
        });
    }

    public async Task<MessagesResponse> PostMessagesAsync(MessagesRequest request)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = AnthropicConstants.Host,
            Path = AnthropicConstants.PostMessagesPath,
        }.ToString();

        request.Stream = false;
        var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadFromJsonAsync<MessagesResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    public async IAsyncEnumerable<StreamingMessagesResponse> PostStreamingMessagesAsync(MessagesRequest request)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = AnthropicConstants.Host,
            Path = AnthropicConstants.PostMessagesPath,
        }.ToString();

        request.Stream = true;
        var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            Console.WriteLine(line);
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
