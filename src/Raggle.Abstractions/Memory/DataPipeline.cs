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

    public required List<string> Steps { get; set; }

    public List<string> RemainingSteps { get; set; } = [];

    public List<string> CompletedSteps { get; set; } = [];

    public required DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? FailedAt { get; set; }

    public string? Message { get; set; }

    [JsonIgnore]
    public object? Context { get; set; }

    public void InitializeSteps()
    {
        RemainingSteps = [.. Steps];
        CompletedSteps.Clear();
    }

    public string? GetPreviousStepName()
    {
        return CompletedSteps.LastOrDefault();
    }

    public string? GetNextStepName()
    {
        return RemainingSteps.FirstOrDefault();
    }

    public void CompleteStep(string stepName)
    {
        if (RemainingSteps.FirstOrDefault() != stepName)
        {
            throw new InvalidOperationException($"완료할 수 없는 단계입니다: {stepName}");
        }
        else if (RemainingSteps.Remove(stepName))
        {
            CompletedSteps.Add(stepName);
        }
        else
        {
            throw new InvalidOperationException($"단계를 완료할 수 없습니다: {stepName}");
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
