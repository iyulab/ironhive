namespace Raggle.Abstractions.Memory.Vector;

public class ScoredVectorPoint
{
    public required string DocumentId { get; set; }

    public required float Score { get; set; }

    public required string Text { get; set; }
}
