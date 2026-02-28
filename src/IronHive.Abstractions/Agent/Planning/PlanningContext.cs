namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Context provided to the planner for plan creation.
/// </summary>
public sealed class PlanningContext
{
    public string? WorkingPath { get; init; }
    public IReadOnlyList<string>? AvailableTools { get; init; }
    public IReadOnlyList<string>? SelectedItems { get; init; }
}
