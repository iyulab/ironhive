namespace IronHive.Abstractions.Vector;

/// <summary>
/// Vector Storage에서 사용할 필터링 조건을 나타냅니다.
/// </summary>
public class VectorRecordFilter
{
    private readonly HashSet<string> _vectorIds = [];
    private readonly HashSet<string> _sourceIds = [];

    public VectorRecordFilter()
    { }

    public VectorRecordFilter(
        IEnumerable<string>? sourceIds = null,
        IEnumerable<string>? vectorIds = null)
    {
        if (sourceIds != null)
            _sourceIds = [.. sourceIds];
        if (vectorIds != null)
            _vectorIds = [.. vectorIds];
    }

    /// <summary>
    /// 필터링할 벡터 레코드 ID 집합입니다.
    /// </summary>
    public IReadOnlyCollection<string> VectorIds => _vectorIds;

    /// <summary>
    /// 필터링할 메모리 소스 ID 집합입니다.
    /// </summary>
    public IReadOnlyCollection<string> SourceIds => _sourceIds;

    /// <summary>
    /// 필터에 내용이 있는지 확인합니다.
    /// </summary>
    public bool Any()
    {
        return _sourceIds.Count != 0 || _vectorIds.Count != 0;
    }

    /// <summary>
    /// 필터에 메모리 소스 ID를 추가합니다. (O(1) 성능)
    /// </summary>
    public void AddSourceId(string sourceId)
    {
        _sourceIds.Add(sourceId); // HashSet handles duplicates automatically
    }

    /// <summary>
    /// 필터에 여러 메모리 소스 ID를 추가합니다.
    /// </summary>
    public void AddSourceIds(IEnumerable<string> sourceIds)
    {
        _sourceIds.UnionWith(sourceIds);
    }

    /// <summary>
    /// 필터에 벡터 레코드 ID를 추가합니다. (O(1) 성능)
    /// </summary>
    public void AddVectorId(string vectorId)
    {
        _vectorIds.Add(vectorId); // HashSet handles duplicates automatically
    }

    /// <summary>
    /// 필터에 여러 벡터 레코드 ID를 추가합니다.
    /// </summary>
    public void AddVectorIds(IEnumerable<string> vectorIds)
    {
        _vectorIds.UnionWith(vectorIds);
    }
}
