using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.LiteDB;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddLiteDBServices(
        this IServiceCollection services,
        object key,
        LiteDBConfig config)
    {
        return services.AddVectorStorage(key, new LiteDBVectorStorage(config));
    }
}
