using Raggle.Engines.OpenAI.Configurations;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Raggle.Engines.OpenAI.Abstractions;

namespace Raggle.Engines.OpenAI.ChatCompletion;

public class OpenAIChatCompletionClient : OpenAIClientBase
{
    public OpenAIChatCompletionClient(string apiKey) : base(apiKey) { }

    public OpenAIChatCompletionClient(OpenAIConfig config) : base(config) { }

    public async Task<IEnumerable<OpenAIChatCompletionModel>> GetChatCompletionModelsAsync()
    {
        var models = await GetModelsAsync();
        return models.Where(OpenAIModel.IsChatCompletionModel)
                     .Select(m => new OpenAIChatCompletionModel
                     {
                         ID = m.ID,
                         Created = m.Created,
                         OwnedBy = m.OwnedBy
                     });
    }

    public async Task<ChatCompletionResponse> PostChatCompletionAsync(ChatCompletionRequest request)
    {
        request.Stream = false;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(OpenAIConstants.PostChatCompletionPath, content);
        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    public async IAsyncEnumerable<StreamingChatCompletionResponse> PostStreamingChatCompletionAsync(ChatCompletionRequest request)
    {
        request.Stream = true;
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(OpenAIConstants.PostChatCompletionPath, content);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data"))
                continue;

            var data = line.Substring("data:".Length).Trim();
            if (!data.StartsWith('{') || !data.EndsWith('}'))
                continue;

            var message = JsonSerializer.Deserialize<StreamingChatCompletionResponse>(data, _jsonOptions);
            if (message != null)
            {
                yield return message;
            }
        }
    }
}
