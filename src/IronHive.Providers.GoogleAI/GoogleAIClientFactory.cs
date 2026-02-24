using Google.GenAI;

namespace IronHive.Providers.GoogleAI;

internal static class GoogleAIClientFactory
{
    internal static Client CreateClient(GoogleAIConfig config)
    {
        return new Client(
            vertexAI: false,
            apiKey: config.ApiKey, 
            httpOptions: config.HttpOptions);
    }

    internal static Client CreateClient(VertexAIConfig config)
    {
        return new Client(
            vertexAI: true,
            credential: config.Credential, 
            project: config.Project,
            location: config.Location,
            httpOptions: config.HttpOptions);
    }
}
