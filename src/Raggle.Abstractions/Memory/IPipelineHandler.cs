namespace Raggle.Abstractions.Memory;

public interface IPipelineHandler
{
    /// <summary>
    /// Process the given pipeline asynchronously.
    /// </summary>
    /// <param name="pipeline">The data pipeline to be processed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the processed <see cref="DataPipeline"/>.</returns>
    Task<DataPipeline> ProcessAsync(
        DataPipeline pipeline, 
        CancellationToken cancellationToken = default);
}
