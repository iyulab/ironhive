using Raggle.Abstractions.Json;
using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Memory;

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
    [JsonInclude]
    public PipelineStatus Status { get; private set; } = PipelineStatus.Queued;

    public required string CollectionName { get; set; }

    public required string DocumentId { get; set; }

    public required string FileName { get; set; }

    public required string MimeType { get; set; }

    [JsonInclude]
    public string? CurrentStep { get; private set; }

    public required List<string> Steps { get; set; } = [];

    public IDictionary<string, object>? Options { get; set; }

    public IEnumerable<string>? Tags { get; set; }

    [JsonInclude]
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    [JsonInclude]
    public DateTime? StartedAt { get; private set; }

    [JsonInclude]
    public DateTime? CompletedAt { get; private set; }

    [JsonInclude]
    public DateTime? FailedAt { get; private set; }

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
        CompletedAt = DateTime.UtcNow;
        return this;
    }

    public DataPipeline Failed(string message)
    {
        Status = PipelineStatus.Failed;
        FailedAt = DateTime.UtcNow;
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
        if (Options == null || CurrentStep == null)
            return default;

        if (Options.TryGetValue<T>(CurrentStep, out var obj))
        {
            return obj;
        }
        return default;
    }
}
