namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Evaluates step results to decide whether to continue, replan, or abort.
/// </summary>
public interface IPlanEvaluator
{
    /// <summary>
    /// Evaluates a completed step and decides the next action.
    /// </summary>
    Task<EvaluationResult> EvaluateAsync(
        TaskPlan plan,
        PlanStep completedStep,
        StepResult result,
        CancellationToken cancellationToken = default);
}
