namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// A single step within a <see cref="TaskPlan"/>.
/// </summary>
public sealed class PlanStep
{
    /// <summary>
    /// The step's number, unique within its plan — the key step events report (<c>StepStarted</c>, <c>StepCompleted</c>).
    /// A planner numbers steps by their 1-based position in <see cref="TaskPlan.Steps"/>.
    /// </summary>
    public int Index { get; init; }
    public required string Description { get; init; }
    public required string Instruction { get; init; }
    public string[] RequiredTools { get; init; } = [];
    /// <summary>The <see cref="Index"/> of each step that must succeed before this one runs.</summary>
    public int[] DependsOn { get; init; } = [];
    public StepStatus Status { get; set; } = StepStatus.Pending;
    public string? Result { get; set; }
}
