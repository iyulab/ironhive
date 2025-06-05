using IronHive.Storages.Azure;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds the Azure Blob Storage provider to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddAzureBlobStorage(
        this IHiveServiceBuilder builder,
        string name,
        AzureBlobConfig config)
    {
        builder.AddFileStorage(new AzureBlobFileStorage(config)
        {
            StorageName = name
        });
        return builder;
    }
}
