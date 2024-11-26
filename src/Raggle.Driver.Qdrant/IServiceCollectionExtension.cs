using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.Qdrant;

public static partial class IServiceCollectionExtension
{
    public static IServiceCollection SetQdrantVectorStorage(
        this IServiceCollection services,
        QdrantConfig config)
    {
        var vectorStorage = new QdrantVectorStorage(config);
        return services.SetVectorStorage(vectorStorage);
    }
}
