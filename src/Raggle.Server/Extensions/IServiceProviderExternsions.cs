using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Raggle.Server.Data;

namespace Raggle.Server.Extensions;

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
