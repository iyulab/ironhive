using System.Collections;
using System.Diagnostics.CodeAnalysis;
using IronHive.Abstractions.Tools;

namespace IronHive.Extensions.AI;

/// <summary>
/// The tool set of one adapted request: built once from the caller's <c>ChatOptions.Tools</c> and never changed while
/// the request runs. Keys are <see cref="ITool.UniqueName"/>, case-insensitive; enumeration keeps the caller's order.
/// </summary>
/// <remarks>
/// Providers only read this set (<see cref="TryGet"/>, enumeration, <see cref="FilterBy"/>); every mutating member
/// throws <see cref="NotSupportedException"/> and <see cref="IsReadOnly"/> says so.
/// </remarks>
internal sealed class ReadOnlyToolCollection : IToolCollection
{
    private readonly List<ITool> _ordered;
    private readonly Dictionary<string, ITool> _byName;

    /// <exception cref="ArgumentException">Two tools share a <see cref="ITool.UniqueName"/> (compared case-insensitively).</exception>
    public ReadOnlyToolCollection(IEnumerable<ITool> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);
        _ordered = [];
        _byName = new Dictionary<string, ITool>(StringComparer.OrdinalIgnoreCase);
        foreach (var tool in tools)
        {
            ArgumentNullException.ThrowIfNull(tool);
            if (!_byName.TryAdd(tool.UniqueName, tool))
                throw new ArgumentException($"An item with the same key already exists. Key: '{tool.UniqueName}'.", nameof(tools));
            _ordered.Add(tool);
        }
    }

    /// <inheritdoc />
    public int Count => _ordered.Count;

    /// <inheritdoc />
    public bool IsReadOnly => true;

    /// <inheritdoc />
    public IReadOnlyCollection<string> Keys => _ordered.Select(t => t.UniqueName).ToArray();

    /// <inheritdoc />
    public bool TryGet(string key, [MaybeNullWhen(false)] out ITool item)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _byName.TryGetValue(key, out item);
    }

    /// <inheritdoc />
    public bool ContainsKey(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return _byName.ContainsKey(key);
    }

    /// <inheritdoc />
    public bool Contains(ITool item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _byName.TryGetValue(item.UniqueName, out var found) && ReferenceEquals(found, item);
    }

    /// <inheritdoc />
    public IToolCollection FilterBy(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);
        var wanted = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
        return new ReadOnlyToolCollection(_ordered.Where(t => wanted.Contains(t.UniqueName)));
    }

    /// <inheritdoc />
    public void CopyTo(ITool[] array, int arrayIndex) => _ordered.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<ITool> GetEnumerator() => _ordered.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Add(ITool item) => throw ReadOnly();

    /// <inheritdoc />
    public void AddRange(IEnumerable<ITool> items) => throw ReadOnly();

    /// <inheritdoc />
#pragma warning disable CA1716
    public void Set(ITool item) => throw ReadOnly();
#pragma warning restore CA1716

    /// <inheritdoc />
    public void SetRange(IEnumerable<ITool> items) => throw ReadOnly();

    /// <inheritdoc />
    public bool Remove(string key) => throw ReadOnly();

    /// <inheritdoc />
    public bool Remove(ITool item) => throw ReadOnly();

    /// <inheritdoc />
    public int RemoveAll(Predicate<ITool>? match = null) => throw ReadOnly();

    /// <inheritdoc />
    public void Clear() => throw ReadOnly();

    private static NotSupportedException ReadOnly() =>
        new("The tool set of an adapted request is read-only; configure tools on the ChatOptions passed to the chat client.");
}
