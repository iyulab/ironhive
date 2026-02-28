namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Creates and revises structured execution plans from goals.
/// </summary>
public interface ITaskPlanner
{
    /// <summary>
    /// Decomposes a goal into a structured plan with ordered steps.
    /// </summary>
    Task<TaskPlan> CreatePlanAsync(
        string goal,
        PlanningContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revises the current plan based on execution failures.
    /// </summary>
    Task<TaskPlan> ReplanAsync(
        TaskPlan currentPlan,
        string failureReason,
        CancellationToken cancellationToken = default);
}
