using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raggle.Engines.Anthropic;

/// <summary>
/// a search client for interacting with Anthropic models.
/// </summary>
public class AnthropicClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _options = new() 
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// creates a new instance of the <see cref="AnthropicClient"/> class.
    /// </summary>
    public AnthropicClient(AnthropicConfig config)
    {
        _client = CreateHttpClient(config);
    }

    /// <summary>
    /// Gets the predefined Anthropic models.
    /// This list of models was last updated on 2024-08-28.
    /// Since there is no API provided by Anthropic to dynamically retrieve the model list,
    /// it is important to manually update this method when new models are available or old models are deprecated.
    /// </summary>
    /// <returns>An enumerable collection of <see cref="AnthropicModel"/> objects.</returns>
    public IEnumerable<AnthropicModel> GetChatModels()
    {
        return AnthropicConstants.PredefinedChatModels;
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
        var content = new StringContent(JsonSerializer.Serialize(request, _options), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();
        var message = await response.Content.ReadFromJsonAsync<MessagesResponse>() 
            ?? throw new InvalidOperationException("Failed to deserialize response.");
        return message;
    }

    public IAsyncEnumerable<MessagesResponse> PostChatStreamAsync(MessagesRequest request)
    {
        var requestUri = new UriBuilder
        {
            Host = AnthropicConstants.Host,
            Path = AnthropicConstants.PostMessagesPath,
        }.Uri;
        request.Stream = true;
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = _client.PostAsync(requestUri, content);
        return JsonSerializer.DeserializeAsyncEnumerable<MessagesResponse>(response.Result.Content.ReadAsStream());
    }

    private static HttpClient CreateHttpClient(AnthropicConfig config)
    {
        if (string.IsNullOrEmpty(config.ApiKey))
            throw new ArgumentException("API key is required.");
        if (string.IsNullOrEmpty(config.Version))
            throw new ArgumentException("Version is required.");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add(AnthropicConstants.AuthHeaderName, config.ApiKey);
        client.DefaultRequestHeaders.Add(AnthropicConstants.VersionHeaderName, config.Version);
        return client;
    }
}
