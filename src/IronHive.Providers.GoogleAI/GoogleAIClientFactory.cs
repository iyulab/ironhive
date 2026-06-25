using Google.GenAI;
using Google.GenAI.Types;

namespace IronHive.Providers.GoogleAI;

internal static class GoogleAIClientFactory
{
    internal static Client Create(GoogleAIConfig config)
    {
        return new Client(
            vertexAI: false,
            apiKey: config.ApiKey, 
            httpOptions: config.HttpOptions,
            clientOptions: config.HttpClientFactory != null ? new ClientOptions
            {
                HttpClientFactory = config.HttpClientFactory
            } : null);
    }

    internal static Client Create(VertexAIConfig config)
    {
        return new Client(
            vertexAI: true,
            credential: config.Credential, 
            project: config.Project,
            location: config.Location,
            httpOptions: config.HttpOptions,
            clientOptions: config.HttpClientFactory != null ? new ClientOptions
            {
                HttpClientFactory = config.HttpClientFactory
            } : null);
    }
}
