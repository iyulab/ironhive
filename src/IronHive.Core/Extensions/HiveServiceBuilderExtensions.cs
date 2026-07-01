using IronHive.Abstractions;
using IronHive.Core.Storages;

namespace IronHive.Core.Extensions;

public static class HiveServiceBuilderExtensions
{
    /// <summary>
    /// 로컬 디스크 파일 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalFileStorage(
        this IHiveServiceBuilder builder,
        string storageName)
    {
        builder.AddFileStorage(storageName, new LocalFileStorage());
        return builder;
    }

    /// <summary>
    /// 로컬 벡터 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalVectorStorage(
        this IHiveServiceBuilder builder,
        string storageName,
        LocalVectorConfig config)
    {
        builder.AddVectorStorage(storageName, new LocalVectorStorage(config));
        return builder;
    }

    /// <summary>
    /// 로컬 큐 스토리지를 등록합니다.
    /// </summary>
    public static IHiveServiceBuilder AddLocalQueueStorage(
        this IHiveServiceBuilder builder,
        string storageName,
        LocalQueueConfig config)
    {
        builder.AddQueueStorage(storageName, new LocalQueueStorage(config));
        return builder;
    }

}
