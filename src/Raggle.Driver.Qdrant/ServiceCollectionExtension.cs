using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.Qdrant;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection SetQdrantVectorStorage(
        this IServiceCollection services,
        QdrantConfig config)
    {
        return services.SetVectorStorage(new QdrantVectorStorage(config));
    }
}
