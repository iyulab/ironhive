using Raggle.Storages.AzureBlob;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection SetAzureBlobDocumentStorage(
        this IServiceCollection services,
        AzureBlobConfig config)
    {
        var documentStorage = new AzureBlobDocumentStorage(config);
        return services.SetDocumentStorage(documentStorage);
    }
}
