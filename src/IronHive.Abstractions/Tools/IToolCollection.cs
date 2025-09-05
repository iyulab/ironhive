namespace IronHive.Abstractions.Tools;

/// <summary>
/// Hive 에이전트가 사용할 수 있는 도구(툴) 컬렉션을 관리합니다.
/// </summary>
public interface IToolCollection : IKeyedCollection<string, ITool>
{
    /// <summary>
    /// 주어진 집합(<paramref name="names"/>)에 포함된 도구만 필터링하여 
    /// 새로운 <see cref="IToolCollection"/>을 반환합니다. 
    /// 존재하지 않는 키는 무시됩니다.
    /// </summary>
    IToolCollection FilterBy(IEnumerable<string> names);
}
