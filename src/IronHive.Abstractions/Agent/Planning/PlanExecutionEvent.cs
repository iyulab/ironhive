namespace IronHive.Abstractions.Agent.Planning;

/// <summary>
/// Events emitted during plan execution for progress tracking.
/// </summary>
public abstract record PlanExecutionEvent;

public sealed record PlanCreatedEvent(TaskPlan Plan) : PlanExecutionEvent;
public sealed record StepStartedEvent(int StepIndex, string Description) : PlanExecutionEvent;
public sealed record StepProgressEvent(int StepIndex, string TextDelta) : PlanExecutionEvent;
public sealed record StepToolCallEvent(int StepIndex, string ToolName, bool Success) : PlanExecutionEvent;
public sealed record StepCompletedEvent(int StepIndex, StepResult Result) : PlanExecutionEvent;
public sealed record PlanReplanEvent(string Reason, TaskPlan NewPlan) : PlanExecutionEvent;
public sealed record PlanCompletedEvent(TaskPlan Plan, string Summary) : PlanExecutionEvent;
public sealed record PlanAbortedEvent(TaskPlan Plan, string Reason) : PlanExecutionEvent;
