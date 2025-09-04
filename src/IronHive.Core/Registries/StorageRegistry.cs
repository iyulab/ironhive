using IronHive.Abstractions.Files;
using IronHive.Abstractions.Queue;
using IronHive.Abstractions.Registries;
using IronHive.Abstractions.Vector;

namespace IronHive.Core.Registries;

/// <inheritdoc />
public class StorageRegistry : KeyedServiceRegistry<string, IStorageItem>, IStorageRegistry
{ 
    public StorageRegistry() : base() { }

    public StorageRegistry(StringComparer comparer) : base(comparer) { }

    /// <inheritdoc />
    public void SetFileStorage(string storageName, IFileStorage storage)
        => Set<IFileStorage>(storageName, storage);

    /// <inheritdoc />
    public void SetQueueStorage(string storageName, IQueueStorage storage)
        => Set<IQueueStorage>(storageName, storage);

    /// <inheritdoc />
    public void SetVectorStorage(string storageName, IVectorStorage storage)
        => Set<IVectorStorage>(storageName, storage);
}
