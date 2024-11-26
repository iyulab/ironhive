using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

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
