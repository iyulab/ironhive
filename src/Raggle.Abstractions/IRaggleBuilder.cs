using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Memory;

namespace Raggle.Abstractions;

public interface IRaggleBuilder
{
    IServiceCollection Services { get; }

    IRaggle Build(RaggleMemoryConfig? config = null);
}
