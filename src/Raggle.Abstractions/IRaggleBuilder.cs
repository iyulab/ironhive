using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Abstractions;

public interface IRaggleBuilder
{
    IServiceCollection Services { get; }

    IRaggle Build();
}
