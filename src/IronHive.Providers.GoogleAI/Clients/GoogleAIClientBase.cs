using IronHive.Abstractions.Http;

namespace IronHive.Providers.GoogleAI.Clients;

internal abstract class GoogleAIClientBase : ProviderClientBase
{
    protected GoogleAIClientBase(GoogleAIConfig config) : base(config)
    {
    }

    protected GoogleAIClientBase(string apiKey) : base(new GoogleAIConfig { ApiKey = apiKey })
    {
    }
}