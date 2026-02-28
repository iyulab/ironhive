namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// A structured execution plan decomposed into ordered steps.
/// </summary>
public sealed class TaskPlan
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public required string Goal { get; init; }
    public string? Reasoning { get; init; }
    public IList<PlanStep> Steps { get; init; } = [];
    public PlanStatus Status { get; set; } = PlanStatus.Pending;
    public int CurrentStepIndex { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public int ReplanCount { get; set; }
}
