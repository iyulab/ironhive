using Raggle.Abstractions.Memory;

namespace Raggle.Abstractions;

public interface IRaggle
{
    IServiceProvider Services { get; }

    IRaggleMemory? Memory { get; }
}
