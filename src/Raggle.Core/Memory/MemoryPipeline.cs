using Raggle.Abstractions.Memory;

namespace Raggle.Core.Memory;

public class MemoryPipeline
{
    private readonly List<object> _steps = new List<object>();

    public MemoryPipeline AddStep<TInput, TOutput>(IPipelineStep<TInput, TOutput> step)
    {
        _steps.Add(step);
        return this;
    }

    public async Task<object> ExecuteAsync(object input)
    {
        object current = input;

        foreach (var step in _steps)
        {
            var stepType = step.GetType();
            var interfaceType = stepType.GetInterface("IPipelineStep`2");
            if (interfaceType == null)
                throw new InvalidOperationException("All steps must implement IPipelineStep<TInput, TOutput>");

            var method = interfaceType.GetMethod("ProcessAsync");
            current = await (Task<object>)method.Invoke(step, new object[] { current });
        }

        return current;
    }
}
