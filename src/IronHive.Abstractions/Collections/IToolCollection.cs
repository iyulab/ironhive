using IronHive.Abstractions.Tools;

namespace IronHive.Abstractions.Collections;

/// <summary>
/// Hive 에이전트가 사용할 수 있는 도구(툴)들을 관리하는 데 사용됩니다.
/// </summary>
public interface IToolCollection : ICollection<ITool>
{
    /// <summary>
    /// 주어진 키 집합(<paramref name="keys"/>)에 포함된 도구만 필터링하여
    /// 새로운 <see cref="IToolCollection"/>을 반환합니다.
    /// </summary>
    /// <param name="keys">포함할 도구의 키 집합.</param>
    /// <returns>조건에 맞는 도구만 포함하는 새로운 컬렉션.</returns>
    IToolCollection WhereBy(IEnumerable<string> keys);

    /// <summary>
    /// 지정한 키에 해당하는 도구가 승인이 필요한지 여부를 반환합니다.
    /// </summary>
    /// <param name="key">확인할 도구의 키.</param>
    bool RequiresApproval(string key);
}
