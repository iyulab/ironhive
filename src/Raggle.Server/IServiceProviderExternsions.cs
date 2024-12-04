using Microsoft.Extensions.DependencyInjection;
using Raggle.Server.Data;

namespace Raggle.Server;

public static partial class IServiceProviderExtensions
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
