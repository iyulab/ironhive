using Raggle.Storages.LocalDisk;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetLocalDiskDocumentStorage(
        this IServiceCollection services,
        LocalDiskConfig config)
    {
        var documentStorage = new LocalDiskDocumentStorage(config);
        return services.SetDocumentStorage(documentStorage);
    }
}
