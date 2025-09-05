namespace IronHive.Core.Utilities;

/// <summary>
/// 최대치가 정해진 카운터.
/// </summary>
public class LimitedCounter
{
    /// <summary>
    /// 현재 카운트가 최대치에 도달했는지 여부
    /// </summary>
    public bool HasLimit => Count >= MaximumCount;

    /// <summary>
    /// 현재 카운트 값
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// 최대 카운트 값
    /// </summary>
    public int MaximumCount { get; }

    /// <summary>
    /// 남은 카운트 수
    /// </summary>
    public int RemainingCount => MaximumCount - Count;

    /// <summary>
    /// 다음 카운트가 가능한지 여부
    /// </summary>
    public bool CanIncrement => Count < MaximumCount;

    public LimitedCounter(int max)
    {
        Count = 0;
        MaximumCount = max < 1 ? 1 : max;
    }

    /// <summary>
    /// 카운트를 증가시킵니다.
    /// </summary>
    /// <returns>증가 성공 여부</returns>
    public bool TryIncrement()
    {
        if (CanIncrement)
        {
            Count++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 카운트를 초기화합니다.
    /// </summary>
    public void Reset()
    {
        Count = 0;
    }
}
