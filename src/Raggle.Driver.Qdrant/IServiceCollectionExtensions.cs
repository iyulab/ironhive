using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Driver.Qdrant;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection SetQdrantVectorStorage(
        this IServiceCollection services,
        QdrantConfig config)
    {
        var vectorStorage = new QdrantVectorStorage(config);
        return services.SetVectorStorage(vectorStorage);
    }
}
