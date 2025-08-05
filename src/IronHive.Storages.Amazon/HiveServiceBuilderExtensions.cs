using IronHive.Storages.Amazon;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Amazon S3 스토리지를 Hive 서비스에 추가합니다.
    /// </summary>
    public static IHiveServiceBuilder AddAmazonS3Storage(
        this IHiveServiceBuilder builder,
        string storageName,
        AmazonS3Config config)
    {
        builder.AddFileStorage(new AmazonS3FileStorage(config)
        {
            StorageName = storageName
        });
        return builder;
    }
}
