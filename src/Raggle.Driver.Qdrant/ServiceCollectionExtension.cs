using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.Qdrant;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddQdrantServices(
        this IServiceCollection services,
        object key,
        QdrantConfig config)
    {
        return services.AddVectorStorage(key, new QdrantVectorStorage(config));
    }
}
