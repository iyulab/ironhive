namespace IronHive.Abstractions.Workflow;

/// <summary>
/// 
/// </summary>
public interface IWorkflow<TContext>
{
    /// <summary>
    /// 
    /// </summary>
    public string? Id { get; }

    /// <summary>
    /// 
    /// </summary>
    public Version? Version { get; }

    /// <summary>
    /// 
    /// </summary>
    public event EventHandler<WorkflowEventArgs<TContext>>? Progressed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(
        TContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="context"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunFromAsync(
        string nodeId,
        TContext context,
        CancellationToken cancellationToken = default);
}
