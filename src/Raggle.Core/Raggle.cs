using Raggle.Abstractions;
using Raggle.Abstractions.Memory;
using Raggle.Core.Memory;

namespace Raggle.Core;

public class Raggle : IRaggle
{
    public IServiceProvider Services { get; }
    public IRaggleMemory Memory { get; }

    public Raggle(IServiceProvider services)
    {
        Services = services;
        Memory = new RaggleMemory(services);
    }
}
