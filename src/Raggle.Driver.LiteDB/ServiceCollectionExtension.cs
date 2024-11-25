using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.LiteDB;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection SetLiteDBVectorStorage(
        this IServiceCollection services,
        LiteDBConfig config)
    {
        return services.SetVectorStorage(new LiteDBVectorStorage(config));
    }
}
