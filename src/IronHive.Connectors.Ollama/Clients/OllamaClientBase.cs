using System.Text.Json;

namespace IronHive.Connectors.Ollama.Clients;

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

    protected OllamaClientBase(string baseUrl)
    {
        var config = new OllamaConfig { BaseUrl = baseUrl };
        _client = CreateHttpClient(config);
        _jsonOptions = config.JsonOptions;
    }

    private static HttpClient CreateHttpClient(OllamaConfig config)
    {
        var client = new HttpClient
        {
            BaseAddress = string.IsNullOrEmpty(config.BaseUrl)
                ? new Uri(OllamaConstants.DefaultBaseUrl.EnsureSuffix('/'))
                : new Uri(config.BaseUrl.EnsureSuffix('/')),
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
