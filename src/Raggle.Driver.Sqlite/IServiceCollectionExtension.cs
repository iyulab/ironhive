using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.Sqlite;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection SetSqliteVectorStorage(
        this IServiceCollection services,
        SqliteConfig config)
    {
        var vectorStorage = new SqliteVectorStorage(config);
        return services.SetVectorStorage(vectorStorage);
    }
}
