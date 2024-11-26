using Microsoft.Extensions.DependencyInjection;
using Raggle.Abstractions.Extensions;

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
