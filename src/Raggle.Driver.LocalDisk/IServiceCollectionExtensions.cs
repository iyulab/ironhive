using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Driver.LocalDisk;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection SetLocalDiskDocumentStorage(
        this IServiceCollection services,
        LocalDiskConfig config)
    {
        var documentStorage = new LocalDiskDocumentStorage(config);
        return services.SetDocumentStorage(documentStorage);
    }
}
