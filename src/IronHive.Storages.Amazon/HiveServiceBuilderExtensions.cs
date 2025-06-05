using IronHive.Storages.Amazon;

namespace IronHive.Abstractions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// Adds the Amazon S3 storage provider to the Hive service builder.
    /// </summary>
    public static IHiveServiceBuilder AddAmazonS3Storage(
        this IHiveServiceBuilder builder,
        string name,
        AmazonS3Config config)
    {
        builder.AddFileStorage(new AmazonS3FileStorage(config)
        {
            StorageName = name
        });
        return builder;
    }
}
