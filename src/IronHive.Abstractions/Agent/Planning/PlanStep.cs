namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// A single step within a <see cref="TaskPlan"/>.
/// </summary>
public sealed class PlanStep
{
    public int Index { get; init; }
    public required string Description { get; init; }
    public required string Instruction { get; init; }
    public string[] RequiredTools { get; init; } = [];
    public int[] DependsOn { get; init; } = [];
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public string? Result { get; set; }
}
