using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.AzureBlob;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection SetAzureBlobDocumentStorage(
        this IServiceCollection services,
        AzureBlobConfig config)
    {
        return services.SetDocumentStorage(new AzureBlobDocumentStorage(config));
    }
}
