namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Decision made after evaluating a completed step.
/// </summary>
public sealed class EvaluationResult
{
    public EvaluationAction Action { get; init; }
    public string? Reason { get; init; }
}
