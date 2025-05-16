namespace IronHive.Core.Utilities;

public class LoopCounter
{
    public int Current { get; private set; }

    public int Max { get; }

    public bool HasNext => Current < Max;

    public LoopCounter(int max)
    {
        Current = 0;
        Max = max < 1 ? 1 : max;
    }

    public void Increment()
    {
        Current++;
    }

    public void Reset()
    {
        Current = 0;
    }
}
