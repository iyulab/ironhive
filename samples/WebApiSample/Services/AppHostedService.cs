using IronHive.Abstractions;
using IronHive.Abstractions.Memory;

namespace WebApiSample.Services;

public class AppHostedService : IHostedService
{
    private readonly IPipelineWorker _worker;

    public AppHostedService(IHiveMind hive)
    {
        _worker = hive.CreatePipelineWorker(new PipelineWorkerConfig
        {
            MaxExecutionSlots = 5,
            PollingInterval = TimeSpan.FromSeconds(1)
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _ = _worker.StartAsync(cancellationToken);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _worker.Dispose();
        await Task.CompletedTask;
    }
}
