namespace IronHive.Abstractions.Storages;

/// <summary>
/// Vector Storage에서 사용할 필터링 조건을 나타냅니다.
/// </summary>
public class VectorRecordFilter
{
    public ICollection<string> VectorIds { get; private set; } = new List<string>();
    public ICollection<string> SourceIds { get; private set; } = new List<string>();

    public VectorRecordFilter()
    { }

    public VectorRecordFilter(
        IEnumerable<string>? sourceIds = null, 
        IEnumerable<string>? vectorIds = null)
    {
        if (sourceIds != null)
            SourceIds = sourceIds.ToList();
        if (vectorIds != null)
            VectorIds = vectorIds.ToList();
    }

    /// <summary>
    /// 필터에 내용이 있는지 확인합니다.
    /// </summary>
    public bool Any()
    {
        return SourceIds.Count != 0 || VectorIds.Count != 0;
    }

    /// <summary>
    /// 필터에 메모리 소스 ID를 추가합니다.
    /// </summary>
    public void AddSourceId(string sourceId)
    {
        if (!SourceIds.Contains(sourceId))
            SourceIds.Add(sourceId);
    }

    /// <summary>
    /// 필터에 여러 메모리 소스 ID를 추가합니다.
    /// </summary>
    public void AddSourceIds(IEnumerable<string> sourceIds)
    {
        foreach (var sourceId in sourceIds)
            AddSourceId(sourceId);
    }

    /// <summary>
    /// 필터에 벡터 레코드 ID를 추가합니다.
    /// </summary>
    public void AddVectorId(string vectorId)
    {
        if (!VectorIds.Contains(vectorId))
            VectorIds.Add(vectorId);
    }

    /// <summary>
    /// 필터에 여러 벡터 레코드 ID를 추가합니다.
    /// </summary>
    public void AddVectorIds(IEnumerable<string> vectorIds)
    {
        foreach (var vectorId in vectorIds)
            AddVectorId(vectorId);
    }
}
