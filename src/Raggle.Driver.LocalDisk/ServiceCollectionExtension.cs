using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.LocalDisk;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection SetLocalDiskDocumentStorage(
        this IServiceCollection services,
        LocalDiskConfig config)
    {
        return services.SetDocumentStorage(new LocalDiskDocumentStorage(config));
    }
}
