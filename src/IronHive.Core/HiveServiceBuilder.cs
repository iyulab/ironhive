using Microsoft.Extensions.DependencyInjection;

namespace IronHive.Core;

public class HiveServiceBuilder : HiveBuilderBase<HiveServiceBuilder>
{
    public HiveServiceBuilder(IServiceCollection services) : base(services)
    {
    }
}