using Raggle.Connectors.LiteDB;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetLiteDBVectorStorage(
        this IServiceCollection services,
        LiteDBConfig config)
    {
        var vectorStorage = new LiteDBVectorStorage(config);
        return services.SetVectorStorage(vectorStorage);
    }
}
