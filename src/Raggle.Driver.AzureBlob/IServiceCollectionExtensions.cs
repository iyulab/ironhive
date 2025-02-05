using Microsoft.Extensions.DependencyInjection;

namespace Raggle.Driver.AzureBlob;

public static partial class IServiceCollectionExtensions
{
    public static IServiceCollection SetAzureBlobDocumentStorage(
        this IServiceCollection services,
        AzureBlobConfig config)
    {
        var documentStorage = new AzureBlobDocumentStorage(config);
        return services.SetDocumentStorage(documentStorage);
    }
}
