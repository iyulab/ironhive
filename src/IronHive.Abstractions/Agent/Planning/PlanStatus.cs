namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Status of a task execution plan.
/// </summary>
public enum PlanStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Replanning
}

/// <summary>
/// Status of an individual plan step.
/// </summary>
public enum StepStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Skipped
}

/// <summary>
/// Action to take after evaluating a step result.
/// </summary>
public enum EvaluationAction
{
    Continue,
    Replan,
    Abort
}
