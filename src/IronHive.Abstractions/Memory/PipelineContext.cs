using System.Text.Json.Serialization;

namespace IronHive.Abstractions.Memory;

public enum PipelineStatus
{
    // 대기
    Pending,

    // 준비
    Queued,
    
    // 처리 중
    Processing,
    
    // 완료
    Completed,
    
    // 실패
    Failed
}

public class PipelineContext
{
    [JsonInclude]
    public PipelineStatus Status { get; private set; } = PipelineStatus.Pending;

    [JsonInclude]
    public required string Id { get; init; }

    [JsonInclude]
    public string? CurrentStep { get; private set; }

    [JsonInclude]
    public required List<string> Steps { get; init; } = [];

    [JsonInclude]
    public IDictionary<string, object>? HandlerOptions { get; init; }

    [JsonInclude]
    public required IMemorySource Source { get; init; }

    public object? Content { get; set; }

    [JsonInclude]
    public DateTime? StartedAt { get; private set; }

    [JsonInclude]
    public DateTime? EndedAt { get; private set; }

    [JsonInclude]
    public string? ErrorMessage { get; private set; }

    public PipelineContext Next()
    {
        var nextStep = GetNextStep();
        if (nextStep == null)
            return Complete();

        CurrentStep = nextStep;
        return this;
    }

    public PipelineContext Start()
    {
        if (Steps.Count == 0)
            throw new InvalidOperationException("파이프라인을 시작할 단계가 없습니다.");

        CurrentStep ??= Steps.First();
        Status = PipelineStatus.Processing;
        StartedAt = DateTime.UtcNow;
        return this;
    }

    public PipelineContext Complete()
    {
        CurrentStep = null;
        Status = PipelineStatus.Completed;
        EndedAt = DateTime.UtcNow;
        return this;
    }

    public PipelineContext Failed(string message)
    {
        Status = PipelineStatus.Failed;
        EndedAt = DateTime.UtcNow;
        ErrorMessage = message;
        return this;
    }

    public string? GetPreviousStep()
    {
        if (CurrentStep == null)
            return null;

        var index = Steps.IndexOf(CurrentStep);
        if (index <= 0)
            return null;

        return Steps[index - 1];
    }

    public string? GetNextStep()
    {
        if (CurrentStep == null)
            return null;

        var index = Steps.IndexOf(CurrentStep);
        if (index == -1 || index >= Steps.Count - 1)
            return null;

        return Steps[index + 1];
    }

    public T? GetCurrentOptions<T>()
    {
        if (HandlerOptions == null || CurrentStep == null)
            return default;

        if (HandlerOptions.TryGetValue<T>(CurrentStep, out var obj))
        {
            return obj;
        }
        return default;
    }
}
