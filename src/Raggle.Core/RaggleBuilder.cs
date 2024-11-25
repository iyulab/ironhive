using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions;
using Raggle.Abstractions.Extensions;

namespace Raggle.Core;

public class RaggleBuilder : IRaggleBuilder
{
    public IServiceCollection Services { get; }

    public RaggleBuilder(IServiceCollection? services = null)
    {
        Services = new ServiceCollection();
        if (services is not null)
        {
            Services.CopyRaggleServicesFrom(services);
        }
    }

    public IRaggle Build()
    {
        var provider = Services.BuildServiceProvider();
        return new Raggle(provider);
    }
}
