namespace IronHive.Abstractions.Memory;

public class PipelineWorkerConfig
{
    public required int MaxExecutionSlots { get; set; }

    public required TimeSpan PollingInterval { get; set; }
}
