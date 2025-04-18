using IronHive.Connectors.OpenAI.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace IronHive.Connectors.OpenAI.Clients;

internal class OpenAIModelClient : OpenAIClientBase
{
    internal OpenAIModelClient(OpenAIConfig config) : base(config)
    { }

    internal OpenAIModelClient(string baseUrl) : base(baseUrl)
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
