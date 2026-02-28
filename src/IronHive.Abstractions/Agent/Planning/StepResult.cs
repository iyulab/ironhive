namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Result of executing a single plan step.
/// </summary>
public sealed class StepResult
{
    public bool Success { get; init; }
    public required string Output { get; init; }
    public string? Error { get; init; }
}
