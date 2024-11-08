using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Core;

public class Raggle
{
    public IServiceProvider HostServices { get; }
    public RaggleServiceProvider Services { get; } = new();

    public Raggle(IServiceProvider? serviceProvider = null)
    {
        if (serviceProvider is not null)
            HostServices = serviceProvider;
        else
        {
            var services = new ServiceCollection();
        }
    }
}
