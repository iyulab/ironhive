namespace IronHive.Abstractions.Tools;

/// <summary>
/// LLM 도구(툴)들을 키로 관리하는 컬렉션입니다.
/// </summary>
public interface IToolCollection : IKeyedCollection<ITool>
{
    /// <summary>
    /// 키들에 해당하는 도구들만 포함하는 새로운 도구 컬렉션을 반환합니다.
    /// </summary>
    IToolCollection WhereBy(IEnumerable<string> keys);

    /// <summary>
    /// 해당하는 키의 도구가 승인이 필요한지 여부를 반환합니다.
    /// </summary>
    bool RequiresApproval(string key);
}
