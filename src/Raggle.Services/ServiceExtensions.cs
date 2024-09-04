using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Services;

public static class ServiceExtensions
{
    public static IServiceCollection AddRaggleServices(this IServiceCollection services)
    {
        services.AddSingleton<RaggleService>();
        return services;
    }
}
