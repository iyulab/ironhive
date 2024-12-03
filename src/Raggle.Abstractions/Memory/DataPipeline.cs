using Raggle.Abstractions.Extensions;
using System.Text.Encodings.Web;
using System.Text.Json;
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

public class DataPipelineStatus
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

public class DataPipeline
{
    [JsonInclude]
    public DataPipelineStatus Progress { get; private set; } = new DataPipelineStatus();

    public required string CollectionName { get; set; }

    public required string DocumentId { get; set; }

    public required string FileName { get; set; }

    public required string MimeType { get; set; }

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
        Progress.SetStart();
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
        Progress.SetComplete();   
        return this;
    }

    public DataPipeline Failed(string message)
    {
        Progress.SetFailed(message);
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

    public T? GetCurrentMetadata<T>(JsonSerializerOptions? jsonOptions = null)
    {
        if (Metadata == null || CurrentStep == null)
            return default;

        if (Metadata.TryGetValue(CurrentStep, out var obj))
        {
            if (obj.TryGet<T>(out var value, jsonOptions))
            {
                return value;
            }
        }
        return default;
    }
}
