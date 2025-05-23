namespace IronHive.Core.Utilities;

/// <summary>
/// 최대치가 정해진 계수기.
/// </summary>
public class LimitedCounter
{
    /// <summary>
    /// 현재 카운트 값
    /// </summary>
    public int Current { get; private set; }

    /// <summary>
    /// 최대 카운트 값
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// 남은 카운트 수
    /// </summary>
    public int Remaining => Max - Current;

    /// <summary>
    /// 다음 카운트가 가능한지 여부
    /// </summary>
    public bool HasCapacity => Current < Max;

    /// <summary>
    /// 현재 카운트가 최대치에 도달했는지 여부
    /// </summary>
    public bool ReachedMax => Current >= Max;

    /// <summary>
    /// 최대치가 정해진 계수기를 생성합니다.
    /// </summary>
    /// <param name="max">최대 카운트 값</param>
    public LimitedCounter(int max)
    {
        Current = 0;
        Max = max < 1 ? 1 : max;
    }

    /// <summary>
    /// 카운트를 증가시킵니다.
    /// </summary>
    /// <returns>증가 성공 여부</returns>
    public bool Increment()
    {
        if (HasCapacity)
        {
            Current++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 카운트를 초기화합니다.
    /// </summary>
    public void Reset()
    {
        Current = 0;
    }
}
