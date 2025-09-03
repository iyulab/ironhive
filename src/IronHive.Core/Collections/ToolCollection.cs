using IronHive.Abstractions.Collections;
using IronHive.Abstractions.Tools;
using System.Collections;
using System.Collections.Concurrent;

namespace IronHive.Core.Collections;

/// <inheritdoc />
public class ToolCollection : IToolCollection
{
    private readonly ConcurrentDictionary<string, ITool> _tools = new(StringComparer.OrdinalIgnoreCase);

    public int Count => throw new NotImplementedException();

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(ITool item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(ITool item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(ITool[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<ITool> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public bool Remove(ITool item)
    {
        throw new NotImplementedException();
    }

    public bool RequiresApproval(string key)
    {
        throw new NotImplementedException();
    }

    public IToolCollection WhereBy(IEnumerable<string> keys)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
