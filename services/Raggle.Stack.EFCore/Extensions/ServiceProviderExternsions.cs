using Microsoft.Extensions.DependencyInjection;
using Raggle.Stack.WebApi.Data;

namespace Raggle.Stack.EFCore.Extensions;

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
