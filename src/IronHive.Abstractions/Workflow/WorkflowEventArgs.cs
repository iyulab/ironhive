namespace IronHive.Abstractions.Workflow;

public enum WorkflowProgressType
{
    Started,
    OnStepBefore,
    OnStepAfter,
    Completed,
    Faulted,
    Cancelled
}

public sealed class WorkflowEventArgs<TContext> : EventArgs
{
    public string? WorkflowId { get; init; }

    public required WorkflowProgressType Type { get; init; }
    public required TContext Context { get; init; }
    
    public string? NodeId { get; init; }
    public string? StepName { get; init; }

    public string? Message { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public Exception? Exception { get; init; }
}
