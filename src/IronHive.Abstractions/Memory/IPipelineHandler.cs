namespace IronHive.Abstractions.Memory;

public interface IPipelineHandler
{
    /// <summary>
    /// Process the given pipeline asynchronously.
    /// </summary>
    /// <param name="context">The data pipeline to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed <see cref="PipelineRequest"/>.</returns>
    Task<PipelineContext> ProcessAsync(PipelineContext context, CancellationToken cancellationToken = default);
}
