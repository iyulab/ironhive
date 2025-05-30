using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Providers.OpenAI.Catalog;

internal class OpenAIModelsClient : OpenAIClientBase
{
    internal OpenAIModelsClient(OpenAIConfig config) : base(config)
    { }

    internal OpenAIModelsClient(string baseUrl) : base(baseUrl)
    { }

    internal async Task<IEnumerable<OpenAIModel>> GetListModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDoc = await Client.GetFromJsonAsync<JsonDocument>(
            OpenAIConstants.GetModelsPath.RemovePreffix('/'), JsonOptions, cancellationToken);

        var models = jsonDoc?.RootElement.GetProperty("data").Deserialize<IEnumerable<OpenAIModel>>(JsonOptions);

        return models?.OrderByDescending(m => m.Created)
            .ToArray() ?? [];
    }

    internal async Task<OpenAIModel?> GetModelAsync(string modelId, CancellationToken cancellationToken)
    {
        var path = Path.Combine(OpenAIConstants.GetModelsPath, modelId).RemovePreffix('/');
        var model = await Client.GetFromJsonAsync<OpenAIModel>(path, JsonOptions, cancellationToken);
        return model;
    }
}
