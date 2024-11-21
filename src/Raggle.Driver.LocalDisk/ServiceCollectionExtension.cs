using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.LocalDisk;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddLocalDiskServices(
        this IServiceCollection services,
        object key,
        LocalDiskConfig config)
    {
        return services.AddDocumentStorage(key, new LocalDiskDocumentStorage(config));
    }
}
