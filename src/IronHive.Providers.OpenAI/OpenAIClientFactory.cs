using System.ClientModel;
using System.ClientModel.Primitives;
using OpenAI;

namespace IronHive.Providers.OpenAI;

public static class OpenAIClientFactory
{
    public static OpenAIClient Create(OpenAIConfig config)
    {
        var credential = new ApiKeyCredential(config.ApiKey);
        var options = new OpenAIClientOptions();

        if (!string.IsNullOrWhiteSpace(config.BaseUrl))
            options.Endpoint = new Uri(config.BaseUrl.EnsureSuffix('/'));
        if (!string.IsNullOrWhiteSpace(config.Organization))
            options.OrganizationId = config.Organization;
        if (!string.IsNullOrWhiteSpace(config.Project))
            options.ProjectId = config.Project;
        if (config.TimeOut.Ticks > 0)
            options.NetworkTimeout = config.TimeOut;
        if (config.HttpClient != null)
            options.Transport = new HttpClientPipelineTransport(config.HttpClient);

        return new OpenAIClient(credential, options);
    }
}
