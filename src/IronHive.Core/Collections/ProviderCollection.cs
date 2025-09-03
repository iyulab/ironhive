using IronHive.Abstractions.Collections;

namespace IronHive.Core.Collections;

/// <inheritdoc />
public class ProviderCollection : KeyedCollection<string, IProviderItem>, IProviderCollection
{ 
    public ProviderCollection() : base() { }

    public ProviderCollection(StringComparer comparer) : base(comparer) { }

}
