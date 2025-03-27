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

public class DataPipeline
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonInclude]
    public PipelineStatus Status { get; private set; } = PipelineStatus.Queued;

    [JsonInclude]
    public string? CurrentStep { get; private set; }

    public required List<string> Steps { get; set; } = [];

    public IDictionary<string, object>? HandlerOptions { get; set; }

    public IMemorySource? Source { get; set; }

    public object? Payload { get; set; }

    [JsonInclude]
    public DateTime? StartedAt { get; private set; }

    [JsonInclude]
    public DateTime? EndedAt { get; private set; }

    [JsonInclude]
    public string? ErrorMessage { get; private set; }

    public DataPipeline Next()
    {
        var nextStep = GetNextStep();
        if (nextStep == null)
            return Complete();

        CurrentStep = nextStep;
        return this;
    }

    public DataPipeline Start()
    {
        if (Steps.Count == 0)
            throw new InvalidOperationException("파이프라인을 시작할 단계가 없습니다.");

        CurrentStep ??= Steps.First();
        Status = PipelineStatus.Processing;
        StartedAt = DateTime.UtcNow;
        return this;
    }

    public DataPipeline Complete()
    {
        CurrentStep = null;
        Status = PipelineStatus.Completed;
        EndedAt = DateTime.UtcNow;
        return this;
    }

    public DataPipeline Failed(string message)
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
