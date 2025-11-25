using IronHive.Storages.Azure;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Azure Blob Storage를 Hive 서비스에 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddAzureBlobStorage(
        this IHiveServiceBuilder builder,
        string storageName,
        AzureStorageConfig config)
    {
        builder.AddFileStorage(storageName, new AzureBlobFileStorage(config));
        return builder;
    }

    /// <summary>
    /// Azure Files Storage를 Hive 서비스에 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddAzureFilesStorage(
        this IHiveServiceBuilder builder,
        string storageName,
        AzureStorageConfig config)
    {
        builder.AddFileStorage(storageName, new AzureFileShareStorage(config));
        return builder;
    }
}
