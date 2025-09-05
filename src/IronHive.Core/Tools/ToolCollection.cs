using IronHive.Abstractions.Tools;

namespace IronHive.Core.Tools;

/// <inheritdoc />
public class ToolCollection : KeyedCollection<string, ITool>, IToolCollection
{
    private readonly IServiceProvider? _services;

    /// <summary>
    /// 기본 툴 컬렉션을 초기화합니다.
    /// </summary>
    public ToolCollection(IServiceProvider? services = null) 
        : this(Enumerable.Empty<ITool>(), StringComparer.Ordinal, services)
    { }

    /// <summary>
    /// 툴 배열을 받아 초기화합니다.
    /// </summary>
    public ToolCollection(IEnumerable<ITool> tools, StringComparer? comparer = null, IServiceProvider? services = null)
        : base(t => t.UniqueName, tools, comparer)
    {
        _services = services;

        foreach (var tool in tools)
            Add(tool);
    }

    /// <inheritdoc />
    public IToolCollection FilterBy(IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        // 중복 키를 제거하고, 존재하는 항목만 안전하게 복사
        var result = new ToolCollection();
        foreach (var name in new HashSet<string>(names, StringComparer.OrdinalIgnoreCase))
        {
            if (TryGet(name, out var tool))
                result.Set(tool);
        }
        return result;
    }
}
