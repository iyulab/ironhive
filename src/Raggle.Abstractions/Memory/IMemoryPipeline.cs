namespace Raggle.Abstractions.Memory;

public interface IPipelineStep<TInput, TOutput>
{
    Task<TOutput> ProcessAsync(TInput input);
}
