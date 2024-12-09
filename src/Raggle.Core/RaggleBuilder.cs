using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raggle.Abstractions;

namespace Raggle.Core;

public class RaggleBuilder : IRaggleBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();

    public IRaggle Build()
    {
        var provider = Services.BuildServiceProvider();
        return new Raggle(provider);
    }
}
