using System.Text.Json.Serialization;

namespace Raggle.Abstractions.Memory;

public enum DataPipelineStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public class DataPipeline
{
    public DataPipelineStatus Status { get; set; } = DataPipelineStatus.Pending;

    public required string CollectionName { get; set; }

    public required string DocumentId { get; set; }

    public required List<string> Steps { get; set; }

    public List<string> RemainingSteps { get; set; } = [];

    public List<string> CompletedSteps { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public string[]? Tags { get; set; }

    public IDictionary<string, object>? Metadata { get; set; }

    public string? Message { get; set; }

    public string[]? Files { get; set; }

    [JsonIgnore]
    public UploadFile? UploadFile { get; set; }

    public string? GetPreviousStepName()
    {
        return CompletedSteps.LastOrDefault();
    }

    public string? GetNextStepName()
    {
        return RemainingSteps.FirstOrDefault();
    }

    public void InitializeSteps()
    {
        RemainingSteps = Steps.ToList();
        CompletedSteps.Clear();
        LastUpdatedAt = DateTime.UtcNow;
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
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void CompleteStep(string stepName)
    {
        if (RemainingSteps.FirstOrDefault() != stepName)
        {
            throw new InvalidOperationException($"완료할 수 없는 단계입니다: {stepName}");
        }
        else
        {
            RemainingSteps.Remove(stepName);
            CompletedSteps.Add(stepName);
            LastUpdatedAt = DateTime.UtcNow;
        }
    }
}
