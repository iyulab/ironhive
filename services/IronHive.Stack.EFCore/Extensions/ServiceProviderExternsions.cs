using Microsoft.Extensions.DependencyInjection;
using IronHive.Stack.WebApi.Data;

namespace IronHive.Stack.EFCore.Extensions;

public static class IServiceProviderExtensions
{
    public static bool EnsureRaggleServices(this IServiceProvider provider)
    {
        using (var scope = provider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RaggleDbContext>();
            return context.Database.EnsureCreated();
        }
    }
}
