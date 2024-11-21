using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

namespace Raggle.Driver.AzureBlob;

public static partial class ServiceCollectionExtension
{
    public static IServiceCollection AddAzureBlobServices(
        this IServiceCollection services,
        object key,
        AzureBlobConfig config)
    {
        return services.AddDocumentStorage(key, new AzureBlobDocumentStorage(config));
    }
}
