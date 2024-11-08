using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Core;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddRaggleServices(this IServiceCollection services, Action<RaggleBuilder>? setupAction = null)
    {
        services.AddSingleton<Raggle>();
        return services;
    }
}
