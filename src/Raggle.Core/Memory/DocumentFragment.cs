namespace Raggle.Core.Memory;

public class DocumentFragment
{
    public int Index { get; set; } = 0;

    public string? Unit { get; set; }

    public int From { get; set; } = 0;

    public int To { get; set; } = 0;

    public object? Content { get; set; }
}
