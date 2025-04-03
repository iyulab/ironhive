namespace IronHive.Abstractions.Memory;

public class PipelineRequest
{
    public required string Id { get; init; }

    public required IMemorySource Source { get; init; }

    public required List<string> Steps { get; init; }

    public IDictionary<string, object?>? HandlerOptions { get; init; }
}
