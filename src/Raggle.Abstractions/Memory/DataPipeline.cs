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

    public Guid ID { get; set; } = Guid.NewGuid();

    public List<string> Steps { get; set; } = [];

    public List<string> RemainingSteps { get; set; } = [];

    public List<string> CompletedSteps { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    public IDictionary<string, object?> Metadata { get; set; } = new Dictionary<string, object?>();

    public string? Message { get; set; }

    public string? GetNextStep()
    {
        return RemainingSteps.FirstOrDefault();
    }

    public void RollBackStep()
    {
        if (CompletedSteps.Count > 0)
        {
            RemainingSteps.Insert(0, CompletedSteps.Last());
            CompletedSteps.Remove(CompletedSteps.Last());
        }
    }

    public void AddMetadata(string key, object? value)
    {
        if (Metadata.ContainsKey(key))
        {
            Metadata[key] = value;
        }
        else
        {
            Metadata.Add(key, value);
        }
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
        }
    }

    public void Validate()
    {
        //if (FileRequest == null)
        //    throw new InvalidOperationException("FileRequest is required.");
        if (RemainingSteps.Count < 1)
            throw new InvalidOperationException("Steps are required.");
    }
}
