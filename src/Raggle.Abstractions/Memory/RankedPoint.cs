namespace Raggle.Abstractions.Memory;

public class RankedPoint
{
    public required string DocumentId { get; set; }

    public required float Score { get; set; }

    public required string Text { get; set; }
}
