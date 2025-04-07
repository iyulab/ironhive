using IronHive.Abstractions;
using IronHive.Connectors.OpenAI;
using IronHive.Core;
using IronHive.Core.Storages;
using Microsoft.Extensions.Options;
using WebApiSample.Entities;

namespace WebApiSample.Services;

public enum AppStatus
{
    NotReady,
    Ready,
    Configuring,
}

public class AppService
{
    private readonly IOptionsMonitor<ServicesSettings> _monitor;

    public AppService(IOptionsMonitor<ServicesSettings> monitor)
    {
        _monitor = monitor;
        CurrentSettings = monitor.CurrentValue;
        monitor.OnChange(HandleChangeSettings);
    }

    public AppStatus Status { get; private set; } = AppStatus.Configuring;

    public ServicesSettings CurrentSettings { get; private set; }
    
    // Changed될때 이벤트 핸들러
    public EventHandler<ServicesSettings>? OnSettingsChanged;

    private void HandleChangeSettings(ServicesSettings settings)
    {
        Status = AppStatus.Configuring;
    }

    private IHiveMind BuildService()
    {
        var service = new HiveServiceBuilder()
            .AddOpenAIConnectors("openai", CurrentSettings.Connectors.OpenAI)
            .AddOpenAIConnectors("lm-studio", CurrentSettings.Connectors.LMStudio)
            .AddOpenAIConnectors("gpu-stack", CurrentSettings.Connectors.GPUStack)
            .AddFileStorage<LocalFileStorage>("local")
            .AddDefaultFileDecoders()
            .AddDefaultPipelineHandlers()
            .WithQueueStorage(new LocalQueueStorage())
            .WithVectorStorage(new LocalVectorStorage())
            .BuildHiveMind();

        return service;
    }
}
