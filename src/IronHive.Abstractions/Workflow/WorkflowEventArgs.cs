namespace IronHive.Abstractions.Workflow;

public enum WorkflowProgressStatus 
{ 
    Started, 
    Succeeded, 
    Failed 
}

public sealed class WorkflowEventArgs<TContext> : EventArgs
{
    public required WorkflowProgressStatus Status { get; init; }
    public required TContext Context { get; init; }

    public string? NodeId { get; init; }
    public string? Step { get; init; }
    
    public Exception? Exception { get; init; }
}
