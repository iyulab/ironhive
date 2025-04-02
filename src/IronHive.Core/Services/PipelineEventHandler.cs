using IronHive.Abstractions.Memory;
using Microsoft.Extensions.Logging;

namespace IronHive.Core.Services;

public class PipelineEventHandler : IPipelineEventHandler
{
    private readonly ILogger _logger;

    public PipelineEventHandler(ILogger<PipelineEventHandler> logger)
    {
        _logger = logger;
    }

    public virtual Task OnCompletedAsync(PipelineContext context)
    {
        _logger.LogInformation("Pipeline {0} Completed", context.Id);
        return Task.CompletedTask;
    }

    public virtual Task OnFailedAsync(PipelineContext context, Exception exception)
    {
        _logger.LogError(exception, "Pipeline {0} Failed", context.Id);
        return Task.CompletedTask;
    }

    public virtual Task OnStartedAsync(PipelineContext context)
    {
        _logger.LogInformation("Pipeline {0} Started", context.Id);
        return Task.CompletedTask;
    }

    public virtual Task OnStepedAsync(PipelineContext context)
    {
        _logger.LogInformation("Pipeline steped: {0}:{1}", context.Id, context.CurrentStep);
        return Task.CompletedTask;
    }
}
