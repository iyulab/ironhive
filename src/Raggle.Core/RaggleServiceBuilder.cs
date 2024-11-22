using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.AI;
using Raggle.Abstractions.Assistant;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory;

namespace Raggle.Core;

public class RaggleServiceBuilder
{
    public IServiceCollection Services { get; }

    public RaggleServiceBuilder(IServiceCollection services)
    {
        Services = new ServiceCollection();
        if (services != null)
        {
            CopyRaggleServices(services, Services);
        }
    }

    public IRaggleMemory BuildMemory(RaggleMemoryConfig config)
    {
        throw new NotImplementedException();
    }

    public IRaggleAssistant BuildAssistant()
    {
        throw new NotImplementedException();
        //var provider = Services.BuildServiceProvider();

        //return new IRaggleAssistant(provider);
    }

    #region Private Methods

    private static void CopyRaggleServices(IServiceCollection source, IServiceCollection target)
    {
        foreach (var service in source)
        {
            if (IsRaggleService(service) && !target.Contains(service))
            {
                target.Add(service);
            }
        }
    }

    private static bool IsRaggleService(ServiceDescriptor service)
    {
        if (!service.IsKeyedService) return false;

        return service.ServiceType == typeof(IDocumentStorage)
            || service.ServiceType == typeof(IVectorStorage)
            || service.ServiceType == typeof(IChatCompletionService)
            || service.ServiceType == typeof(IEmbeddingService)
            || service.ServiceType == typeof(IDocumentDecoder)
            || service.ServiceType == typeof(IPipelineHandler);
    }

    #endregion
}
