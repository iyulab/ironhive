using IronHive.Abstractions.Collections;

namespace IronHive.Core.Collections;

/// <inheritdoc />
public class StorageCollection : KeyedCollection<string, IStorageItem>, IStorageCollection
{ 
    public StorageCollection() : base() { }

    public StorageCollection(StringComparer comparer) : base(comparer) { }

}
