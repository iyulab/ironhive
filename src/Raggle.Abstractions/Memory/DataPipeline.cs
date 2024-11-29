using Raggle.Abstractions.Extensions;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Memory;

public enum PipelineStatus
{
    // 준비
    Queued,
    // 처리 중
    Processing,
    // 완료
    Completed,
    // 실패
    Failed
}

public record PipelineStatusInfo
{
    [JsonInclude]
    public PipelineStatus Status { get; private set; } = PipelineStatus.Queued;

    [JsonInclude]
    public DateTime? StartedAt { get; private set; }

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? FailedAt { get; private set; }

    [JsonInclude]
    public string? ErrorMessage { get; private set; }

    public void SetStart()
    {
        Status = PipelineStatus.Processing;
        StartedAt = DateTime.UtcNow;
    }

    public void SetComplete()
    {
        Status = PipelineStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void SetFailed(string message)
    {
        Status = PipelineStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = message;
    }
}

public record FileInfo
{
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; } = 0;
    public string ContentType { get; set; } = string.Empty;
}

public class DataPipeline
{
    [JsonInclude]
    public PipelineStatusInfo StatusInfo { get; private set; } = new PipelineStatusInfo();

    public required FileInfo FileInfo { get; init; }

    public required string CollectionName { get; set; } = string.Empty;

    public required string DocumentId { get; set; } = string.Empty;

    [JsonInclude]
    public string? CurrentStep { get; private set; }

    public List<string> Steps { get; set; } = [];

    public IEnumerable<string>? Tags { get; set; }

    public IDictionary<string, object>? Metadata { get; set; }

    [JsonInclude]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DataPipeline Start()
    {
        if (Steps.Count == 0)
            throw new InvalidOperationException("파이프라인을 시작할 단계가 없습니다.");

        CurrentStep ??= Steps.First();
        StatusInfo.SetStart();
        return this;
    }

    public DataPipeline Next()
    {
        var nextStep = GetNextStep();
        if (nextStep == null)
            return Complete();

        CurrentStep = nextStep;
        return this;
    }

    public DataPipeline Complete()
    {
        CurrentStep = null;
        StatusInfo.SetComplete();   
        return this;
    }

    public DataPipeline Failed(string message)
    {
        StatusInfo.SetFailed(message);
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

    public T? GetCurrentMetadata<T>()
    {
        if (Metadata == null || CurrentStep == null)
            return default;

        if (Metadata.TryGetValue<T>(CurrentStep, out var value))
        {
            return value;
        }
        return default;
    }
}
