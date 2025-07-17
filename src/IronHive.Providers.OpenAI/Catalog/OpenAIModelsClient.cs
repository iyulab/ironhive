using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Catalog;

public class OpenAIModelsClient : OpenAIClientBase
{
    public OpenAIModelsClient(OpenAIConfig config) : base(config)
    { }

    public OpenAIModelsClient(string baseUrl) : base(baseUrl)
    { }

    public async Task<IEnumerable<OpenAIModel>> GetListModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var jsonDoc = await Client.GetFromJsonAsync<JsonDocument>(
            OpenAIConstants.GetModelsPath.RemovePreffix('/'), JsonOptions, cancellationToken);

        var models = jsonDoc?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>(JsonOptions);
        return models?.OrderByDescending(m => m.Created)
            .ToArray() ?? [];
    }

    public async Task<OpenAIModel?> GetModelAsync(
        string modelId, 
        CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(OpenAIConstants.GetModelsPath, modelId).RemovePreffix('/');
        var model = await Client.GetFromJsonAsync<OpenAIModel>(path, JsonOptions, cancellationToken);
        return model;
    }
}
