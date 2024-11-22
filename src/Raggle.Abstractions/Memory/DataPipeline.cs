using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Memory;

public enum PipelineStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class DataPipeline
{
    public PipelineStatus Status { get; set; } = PipelineStatus.Pending;

    public required DocumentRecord Document { get; set; }

    public required List<object> Steps { get; set; }

    public List<object> RemainingSteps { get; set; } = [];

    public List<object> CompletedSteps { get; set; } = [];

    public required DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? ErrorMessage { get; set; }

    [JsonIgnore]
    public object? Context { get; set; }

    public void InitializeSteps()
    {
        RemainingSteps = [.. Steps];
        CompletedSteps.Clear();
    }

    public object? GetPreviousStepKey()
    {
        return CompletedSteps.LastOrDefault();
    }

    public object? GetNextStepKey()
    {
        return RemainingSteps.FirstOrDefault();
    }

    public void CompleteStep(object stepKey)
    {
        if (RemainingSteps.FirstOrDefault() != stepKey)
        {
            throw new InvalidOperationException($"완료할 수 없는 단계입니다: {stepKey}");
        }
        else if (RemainingSteps.Remove(stepKey))
        {
            CompletedSteps.Add(stepKey);
        }
        else
        {
            throw new InvalidOperationException($"단계를 완료할 수 없습니다: {stepKey}");
        }
    }

    public void AdjustSteps(int count)
    {
        if (count == 0)
        {
            return; // 조절할 단계가 없음
        }
        else if (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                var step = RemainingSteps.FirstOrDefault();
                if (step == null) break;
                RemainingSteps.RemoveAt(0);
                CompletedSteps.Add(step);
            }
        }
        else if (count < 0)
        {
            count = Math.Abs(count);
            for (int i = 0; i < count; i++)
            {
                var step = CompletedSteps.LastOrDefault();
                if (step == null) break;
                CompletedSteps.RemoveAt(CompletedSteps.Count - 1);
                RemainingSteps.Insert(0, step);
            }
        }
    }

    public bool TryGetContext<T>(out T value)
    {
        if (Context is T result)
        {
            value = result;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }
}
