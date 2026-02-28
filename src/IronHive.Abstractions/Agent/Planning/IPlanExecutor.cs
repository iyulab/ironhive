namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Executes individual plan steps using agent tools.
/// </summary>
public interface IPlanExecutor
{
    /// <summary>
    /// Executes a single plan step, streaming progress events.
    /// </summary>
    IAsyncEnumerable<PlanExecutionEvent> ExecuteStepAsync(
        TaskPlan plan,
        PlanStep planStep,
        CancellationToken cancellationToken = default);
}
