using IronHive.Abstractions.Http;

namespace IronHive.Providers.Ollama.Clients;

internal abstract class OllamaClientBase : ProviderClientBase
{
    protected OllamaClientBase(OllamaConfig? config = null) : base(config ?? new OllamaConfig())
    {
    }

    protected OllamaClientBase(string baseUrl) : base(new OllamaConfig { BaseUrl = baseUrl })
    {
    }
}
