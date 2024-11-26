using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

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
