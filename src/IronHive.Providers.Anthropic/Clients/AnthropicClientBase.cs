using IronHive.Abstractions.Http;

namespace IronHive.Providers.Anthropic.Clients;

public abstract class AnthropicClientBase : ProviderClientBase
{
    protected AnthropicClientBase(AnthropicConfig config) : base(config)
    {
    }

    protected AnthropicClientBase(string apiKey) : base(new AnthropicConfig { ApiKey = apiKey })
    {
    }
}
