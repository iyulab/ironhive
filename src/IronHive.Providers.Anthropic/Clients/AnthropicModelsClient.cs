using IronHive.Providers.Anthropic.Payloads.Models;
using System.Net.Http.Json;

namespace IronHive.Providers.Anthropic.Clients;

public class AnthropicModelsClient : AnthropicClientBase
{
    public AnthropicModelsClient(AnthropicConfig config) : base(config)
    { }

    public AnthropicModelsClient(string apiKey) : base(apiKey)
    { }

    public async Task<ListModelsResponse> GetModelsAsync(
        ListModelsRequest request, 
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(request.BeforeId))
            query["before_id"] = request.BeforeId;
        if (!string.IsNullOrWhiteSpace(request.AfterId))
            query["after_id"] = request.AfterId;
        if (request.Limit > 0)
            query["limit"] = request.Limit.ToString();

        var path = AnthropicConstants.GetModelsPath.RemovePrefix('/');
        if (query.Count != 0)
        {
            path += "?"; 
            path += string.Join("&", query.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}"));
        }

        var response = await _client.GetFromJsonAsync<ListModelsResponse>(path, _jsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve models from Anthropic API.");
        return response;
    }

    public async Task<AnthropicModel?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken)
    {
        var path = Path.Combine(AnthropicConstants.GetModelsPath, modelId).RemovePrefix('/');
        var model = await _client.GetFromJsonAsync<AnthropicModel>(path, _jsonOptions, cancellationToken);
        return model;
    }
}