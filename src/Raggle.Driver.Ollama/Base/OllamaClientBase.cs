using Raggle.Driver.Ollama.Configurations;
using System.Net.Http.Json;
using System.Text.Json;

namespace Raggle.Driver.Ollama.Base;

internal abstract class OllamaClientBase
{
    protected readonly HttpClient _client;
    protected readonly JsonSerializerOptions _jsonOptions;

    protected OllamaClientBase(OllamaConfig? config = null)
    {
        config ??= new OllamaConfig();
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    protected OllamaClientBase(string endPoint)
    {
        var config = new OllamaConfig { EndPoint = endPoint };
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    public async Task<IEnumerable<OllamaModel>> GetModelsAsync(CancellationToken cancellationToken)
    {
        var jsonDocument = await _client.GetFromJsonAsync<JsonDocument>(OllamaConstants.GetModelListPath, _jsonOptions, cancellationToken);
        var models = jsonDocument?.RootElement.GetProperty("models").Deserialize<IEnumerable<OllamaModel>>();
        return models?.OrderByDescending(m => m.ModifiedAt).ToArray() ?? [];
    }

    private static HttpClient CreateHttpClient(OllamaConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrEmpty(config.EndPoint)
                ? new Uri(OllamaConstants.DefaultEndPoint.EnsureSuffix('/'))
                : new Uri(config.EndPoint.EnsureSuffix('/')),
        };

        if (config.DefaultRequestHeaders != null)
        {
            foreach (var header in config.DefaultRequestHeaders)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
        return client;
    }
}
