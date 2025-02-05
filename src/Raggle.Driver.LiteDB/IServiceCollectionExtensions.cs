using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Driver.LiteDB;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection SetLiteDBVectorStorage(
        this IServiceCollection services,
        LiteDBConfig config)
    {
        var vectorStorage = new LiteDBVectorStorage(config);
        return services.SetVectorStorage(vectorStorage);
    }
}
