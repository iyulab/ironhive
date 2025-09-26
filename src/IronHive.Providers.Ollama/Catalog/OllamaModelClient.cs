using IronHive.Providers.Ollama.Catalog.Models;
using IronHive.Providers.Ollama.Share;
using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Providers.Ollama.Catalog;

internal class OllamaModelClient : OllamaClientBase
{
    internal OllamaModelClient(OllamaConfig? config = null) : base(config) 
    { }

    internal OllamaModelClient(string baseUrl) : base(baseUrl) 
    { }

    internal async Task<IEnumerable<OllamaModelSummary>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(
            OllamaConstants.GetListModelsPath, 
            _jsonOptions, 
            cancellationToken);
        
        var models = jsonDocument?.RootElement.GetProperty("models").Deserialize<IEnumerable<OllamaModelSummary>>();

        return models?.OrderByDescending(m => m.ModifiedAt)
            .ToArray() ?? [];
    }

    internal async Task<OllamaModel?> GetModelAsync(string modelId, CancellationToken cancellationToken)
    {
        var req = new OllamaModelGetRequest
        {
            Model = modelId,
            Verbose = true
        };
        var json = JsonSerializer.Serialize(req, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(OllamaConstants.PostModelInfoPath, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var model = await response.Content.ReadFromJsonAsync<OllamaModel>(_jsonOptions, cancellationToken);
        return model;
    }
}
