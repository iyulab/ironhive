using Raggle.Engines.Anthropic.Configurations;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Net.Http.Json;

namespace Raggle.Engines.Anthropic.ChatCompletion;

public class AnthropicChatClient : AnthropicClientBase
{
    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public AnthropicChatClient(AnthropicConfig config) : base(config) { }

    public AnthropicChatClient(string apiKey) : base(apiKey) { }

    /// <summary>
    /// Anthropic does not provide a way to list available models.
    /// so this list is hardcoded models, writed at 2024-09-02.
    /// </summary>
    public IEnumerable<AnthropicModel> GetChatModels()
        => AnthropicConstants.PredefinedChatModels;

    public async Task<MessagesResponse> PostMessagesAsync(MessagesRequest request)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = AnthropicConstants.Host,
            Path = AnthropicConstants.PostMessagesPath,
        }.ToString();

        request.Stream = false;
        var content = new StringContent(JsonSerializer.Serialize(request, _options), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadFromJsonAsync<MessagesResponse>()
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    public IAsyncEnumerable<MessagesResponse> PostStreamingMessagesAsync(MessagesRequest request)
    {
        var requestUri = new UriBuilder
        {
            Scheme = "https",
            Host = AnthropicConstants.Host,
            Path = AnthropicConstants.PostMessagesPath,
        }.ToString();

        request.Stream = true;
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = _client.PostAsync(requestUri, content);
        return JsonSerializer.DeserializeAsyncEnumerable<MessagesResponse>(response.Result.Content.ReadAsStream());
    }
}
