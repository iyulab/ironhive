using IronHive.Providers.Anthropic.Catalog.Models;
using IronHive.Providers.Anthropic.Share;
using System.Net.Http.Json;

namespace IronHive.Providers.Anthropic.Catalog;

public class AnthropicModelsClient : AnthropicClientBase
{
    public AnthropicModelsClient(AnthropicConfig config) : base(config)
    { }

    public AnthropicModelsClient(string apiKey) : base(apiKey)
    { }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    public async Task<AnthropicListModelsResponse> GetModelsAsync(
        AnthropicListModelsRequest request, 
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(request.BeforeId))
            query["before_id"] = request.BeforeId;
        if (!string.IsNullOrWhiteSpace(request.AfterId))
            query["after_id"] = request.AfterId;
        if (request.Limit > 0)
            query["limit"] = request.Limit.ToString();

        var path = AnthropicConstants.GetModelsPath.RemovePreffix('/');
        if (query.Count != 0)
        {
            path += "?"; 
            path += string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));
        }

        var response = await _client.GetFromJsonAsync<AnthropicListModelsResponse>(path, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve models from Anthropic API.");
        return response;
    }

    /// <summary>
    /// Gets the list of Anthropic models.
    /// </summary>
    public async Task<AnthropicModel?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(AnthropicConstants.GetModelsPath, modelId).RemovePreffix('/');
        var model = await _client.GetFromJsonAsync<AnthropicModel>(path, _jsonOptions, cancellationToken);
        return model;
    }
}
