using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory;
using Raggle.Core.Memory.Decoders;

namespace Raggle.Core;

public class RaggleBuilder : IRaggleBuilder
{
    public IServiceCollection Services { get; }

    public RaggleBuilder(IServiceCollection? services = null)
    {
        Services = new ServiceCollection();
        if (services != null)
        {
            CopyRaggleServices(services);
        }
    }

    public IRaggleBuilder AddContentDecoder(object key, IDocumentDecoder decoder)
    {
        Services.AddKeyedSingleton(key, decoder);
        return this;
    }

    public IRaggleBuilder AddEmbeddingService(object key, IEmbeddingService service)
    {
        Services.AddKeyedSingleton(key, service);
        return this;
    }

    public IRaggle Build(RaggleMemoryConfig? config = null)
    {
        var provider = Services.BuildServiceProvider();
        if (config == null)
        {
            return new Raggle()
            {
                Services = provider
            };
        }
        else
        {
            return new Raggle()
            {
                Services = provider
            };
        }
    }

    #region Private Methods

    private void CopyRaggleServices(IServiceCollection source)
    {
        foreach (var service in source)
        {
            if (IsMemoryService(service) && !Services.Contains(service))
            {
                Services.Add(service);
            }
        }
    }

    private bool IsMemoryService(ServiceDescriptor service)
    {
        if (!service.IsKeyedService) return false;

        return service.ServiceType == typeof(IDocumentStorage)
            || service.ServiceType == typeof(IVectorStorage)
            || service.ServiceType == typeof(IChatCompletionService)
            || service.ServiceType == typeof(IEmbeddingService);
    }

    #endregion
}
