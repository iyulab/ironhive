using Raggle.Storages.Qdrant;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetQdrantVectorStorage(
        this IServiceCollection services,
        QdrantConfig config)
    {
        var vectorStorage = new QdrantVectorStorage(config);
        return services.SetVectorStorage(vectorStorage);
    }
}
