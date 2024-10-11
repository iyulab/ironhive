namespace Raggle.Core.Memory;

public interface IPipelineStep<TInput, TOutput>
{
    Task<TOutput> ProcessAsync(TInput input);
}
