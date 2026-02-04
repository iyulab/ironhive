using IronHive.Abstractions.Http;

namespace IronHive.Providers.OpenAI.Clients;

public abstract class OpenAIClientBase : ProviderClientBase
{
    protected OpenAIClientBase(OpenAIConfig config) : base(config)
    {
    }

    protected OpenAIClientBase(string apiKey) : base(new OpenAIConfig { ApiKey = apiKey })
    {
    }
}