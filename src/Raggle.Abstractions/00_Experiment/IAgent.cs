namespace Raggle.Abstractions.Experiment;

public interface IAgent<TInput,TOutput>
{
    Task<TOutput> InvokeAsync(
        TInput input,
        CancellationToken cancellationToken = default);
}
